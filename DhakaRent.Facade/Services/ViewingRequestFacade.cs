
using DhakaRent.Entities.Enums;
using DhakaRent.Entities.Models;
using DhakaRent.Facade.Interfaces;

namespace DhakaRent.Facade.Services
{
    public class ViewingRequestFacade : IViewingRequestFacade
    {
        private readonly IViewingRequestRepository
            _viewingRequestRepository;

        private readonly IPropertyRepository
            _propertyRepository;

        public ViewingRequestFacade(
            IViewingRequestRepository viewingRequestRepository,
            IPropertyRepository propertyRepository)
        {
            _viewingRequestRepository = viewingRequestRepository;
            _propertyRepository = propertyRepository;
        }

        // 1. Get viewing requests submitted by a tenant
        public async Task<IReadOnlyList<ViewingRequest>>
            GetMineAsync(string tenantId)
        {
            ValidateUserId(tenantId);

            return await _viewingRequestRepository
                .GetByTenantIdAsync(tenantId);
        }

        // 2. Get viewing requests received by an owner
        public async Task<IReadOnlyList<ViewingRequest>>
            GetReceivedAsync(string ownerId)
        {
            ValidateUserId(ownerId);

            return await _viewingRequestRepository
                .GetByOwnerIdAsync(ownerId);
        }

        // Retrieve property details for viewing request lists.
        // Property IDs must come from the current user's
        // authorized viewing requests.
        public Task<IReadOnlyDictionary<int, Property>>
            GetPropertiesByIdsAsync(
                IReadOnlyCollection<int> propertyIds)
        {
            // Step A: Validate input.
            ArgumentNullException.ThrowIfNull(propertyIds);

            // Step B: Retrieve properties using the repository.
            return _viewingRequestRepository
                .GetPropertiesByIdsAsync(propertyIds);
        }

        // 3. Get a specific request belonging to a tenant
        public async Task<ViewingRequest?> GetMineByIdAsync(
            int requestId,
            string tenantId)
        {
            ValidateUserId(tenantId);

            if (requestId <= 0)
            {
                return null;
            }

            return await _viewingRequestRepository
                .GetByIdAndTenantAsync(requestId, tenantId);
        }

        // 4. Get a specific request for an owner's property
        public async Task<ViewingRequest?> GetReceivedByIdAsync(
            int requestId,
            string ownerId)
        {
            ValidateUserId(ownerId);

            if (requestId <= 0)
            {
                return null;
            }

            return await _viewingRequestRepository
                .GetByIdAndOwnerAsync(requestId, ownerId);
        }

        // 5. Tenant creates a property viewing request
        public async Task<int> CreateAsync(
            int propertyId,
            string tenantId,
            DateTime requestedDateTimeUtc,
            string? tenantMessage)
        {
            // Step A: Validate tenant ID
            ValidateUserId(tenantId);

            // Step B: Validate property ID
            if (propertyId <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(propertyId),
                    "A valid property ID is required.");
            }

            // Step C: Ensure date/time is UTC
            if (requestedDateTimeUtc.Kind != DateTimeKind.Utc)
            {
                throw new ArgumentException(
                    "Requested date and time must be in UTC.",
                    nameof(requestedDateTimeUtc));
            }

            // Step D: Ensure requested time is in the future
            DateTime now = DateTime.UtcNow;

            if (requestedDateTimeUtc <= now)
            {
                throw new ArgumentException(
                    "Please select a future viewing date and time.",
                    nameof(requestedDateTimeUtc));
            }

            // Step E: Validate optional tenant message
            string? message = tenantMessage?.Trim();

            if (message?.Length > 1000)
            {
                throw new ArgumentException(
                    "Tenant message cannot exceed 1000 characters.",
                    nameof(tenantMessage));
            }

            if (string.IsNullOrWhiteSpace(message))
            {
                message = null;
            }

            // Step F: Retrieve the property
            var property = await _propertyRepository
                .GetByIdAsync(propertyId);

            // Step G: Only approved properties are eligible
            if (property == null ||
                property.ListingStatus != ListingStatus.Approved)
            {
                throw new InvalidOperationException(
                    "This property is not available for viewing requests.");
            }

            // Step H: Prevent inquiries about own property
            if (property.OwnerId == tenantId)
            {
                throw new InvalidOperationException(
                    "You cannot request a viewing of your own property.");
            }

            // Step I: Create the viewing request
            var request = new ViewingRequest
            {
                PropertyId = propertyId,
                TenantId = tenantId,
                RequestedDateTime = requestedDateTimeUtc,
                ProposedDateTime = null,
                TenantMessage = message,
                OwnerResponse = null,
                Status = ViewingRequestStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            // Step J: Save to database
            await _viewingRequestRepository.AddAsync(request);

            await _viewingRequestRepository.SaveChangesAsync();

            // Return the newly generated request ID
            return request.Id;
        }


        // 6. PropertyOwner accepts a viewing request
        public async Task AcceptAsync(
            int requestId,
            string ownerId,
            string? ownerResponse)
        {
            // Step A: Validate logged-in owner ID.
            ValidateUserId(ownerId);

            // Step B: Validate request ID.
            if (requestId <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(requestId),
                    "A valid viewing request ID is required.");
            }

            // Step C: Validate optional owner response.
            string? response = ownerResponse?.Trim();

            if (response?.Length > 1000)
            {
                throw new ArgumentException(
                    "Owner response cannot exceed 1000 characters.",
                    nameof(ownerResponse));
            }

            if (string.IsNullOrWhiteSpace(response))
            {
                response = null;
            }

            // Step D: Retrieve request with owner verification.
            var request = await _viewingRequestRepository
                .GetByIdAndOwnerAsync(
                    requestId,
                    ownerId);

            if (request == null)
            {
                throw new UnauthorizedAccessException(
                    "Viewing request not found or access denied.");
            }

            // Step E: Only pending requests can be accepted.
            if (request.Status != ViewingRequestStatus.Pending)
            {
                throw new InvalidOperationException(
                    "Only pending viewing requests can be accepted.");
            }

            // Step F: Requested time must still be in the future.
            DateTime now = DateTime.UtcNow;

            if (request.RequestedDateTime <= now)
            {
                throw new InvalidOperationException(
                    "The requested viewing time has already passed.");
            }

            // Step G: Accept the requested viewing time.
            request.Status = ViewingRequestStatus.Accepted;

            // Optional response from the property owner.
            request.OwnerResponse = response;

            // No alternative time is needed.
            request.ProposedDateTime = null;

            // Update audit timestamp.
            request.UpdatedAt = now;

            // Step H: Save changes to SQL Server.
            await _viewingRequestRepository
                .SaveChangesAsync();
        }



        // 7. PropertyOwner rejects a viewing request
        public async Task RejectAsync(
            int requestId,
            string ownerId,
            string? ownerResponse)
        {
            // Step A: Validate logged-in owner ID.
            ValidateUserId(ownerId);

            // Step B: Validate viewing request ID.
            if (requestId <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(requestId),
                    "A valid viewing request ID is required.");
            }

            // Step C: Validate optional owner response.
            string? response = ownerResponse?.Trim();

            if (response?.Length > 1000)
            {
                throw new ArgumentException(
                    "Owner response cannot exceed 1000 characters.",
                    nameof(ownerResponse));
            }

            if (string.IsNullOrWhiteSpace(response))
            {
                response = null;
            }

            // Step D: Retrieve the viewing request
            // with PropertyOwner ownership verification.
            var request = await _viewingRequestRepository
                .GetByIdAndOwnerAsync(
                    requestId,
                    ownerId);

            if (request == null)
            {
                throw new UnauthorizedAccessException(
                    "Viewing request not found or access denied.");
            }

            // Step E: Only pending requests can be rejected.
            if (request.Status != ViewingRequestStatus.Pending)
            {
                throw new InvalidOperationException(
                    "Only pending viewing requests can be rejected.");
            }

            // Step F: Update the request status.
            request.Status = ViewingRequestStatus.Rejected;

            // Save the owner's optional explanation.
            request.OwnerResponse = response;

            // No alternative viewing time is proposed.
            request.ProposedDateTime = null;

            // Update audit timestamp.
            request.UpdatedAt = DateTime.UtcNow;

            // Step G: Save changes to SQL Server.
            await _viewingRequestRepository
                .SaveChangesAsync();
        }



        // 8. PropertyOwner proposes an alternative viewing time
        public async Task ProposeRescheduleAsync(
            int requestId,
            string ownerId,
            DateTime proposedDateTimeUtc,
            string? ownerResponse)
        {
            // Step A: Validate logged-in owner ID.
            ValidateUserId(ownerId);

            // Step B: Validate request ID.
            if (requestId <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(requestId),
                    "A valid viewing request ID is required.");
            }

            // Step C: Ensure proposed date/time is UTC.
            if (proposedDateTimeUtc.Kind != DateTimeKind.Utc)
            {
                throw new ArgumentException(
                    "Proposed date and time must be in UTC.",
                    nameof(proposedDateTimeUtc));
            }

            // Step D: Proposed time must be in the future.
            DateTime now = DateTime.UtcNow;

            if (proposedDateTimeUtc <= now)
            {
                throw new ArgumentException(
                    "Please propose a future viewing date and time.",
                    nameof(proposedDateTimeUtc));
            }

            // Step E: Validate optional owner response.
            string? response = ownerResponse?.Trim();

            if (response?.Length > 1000)
            {
                throw new ArgumentException(
                    "Owner response cannot exceed 1000 characters.",
                    nameof(ownerResponse));
            }

            if (string.IsNullOrWhiteSpace(response))
            {
                response = null;
            }

            // Step F: Retrieve the request with ownership verification.
            var request = await _viewingRequestRepository
                .GetByIdAndOwnerAsync(
                    requestId,
                    ownerId);

            if (request == null)
            {
                throw new UnauthorizedAccessException(
                    "Viewing request not found or access denied.");
            }

            // Step G: Only pending requests can be rescheduled.
            if (request.Status != ViewingRequestStatus.Pending)
            {
                throw new InvalidOperationException(
                    "Only pending viewing requests can be rescheduled.");
            }

            // Step H: Alternative time must differ
            // from the tenant's original requested time.
            if (proposedDateTimeUtc == request.RequestedDateTime)
            {
                throw new InvalidOperationException(
                    "The proposed time must be different " +
                    "from the original requested time.");
            }

            // Step I: Save the owner's proposed time.
            request.ProposedDateTime = proposedDateTimeUtc;

            // Preserve the tenant's original requested time.
            // Do not change request.RequestedDateTime here.

            // Step J: Save optional owner explanation.
            request.OwnerResponse = response;

            // Step K: Update request status.
            request.Status =
                ViewingRequestStatus.RescheduleRequested;

            // Step L: Update audit timestamp.
            request.UpdatedAt = DateTime.UtcNow;

            // Step M: Save changes to SQL Server.
            await _viewingRequestRepository
                .SaveChangesAsync();
        }



        // 9. Tenant accepts the owner's proposed viewing time
        public async Task AcceptProposedTimeAsync(
            int requestId,
            string tenantId)
        {
            // Step A: Validate logged-in Tenant ID.
            ValidateUserId(tenantId);

            // Step B: Validate request ID.
            if (requestId <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(requestId),
                    "A valid viewing request ID is required.");
            }

            // Step C: Retrieve the request
            // with Tenant ownership verification.
            var request = await _viewingRequestRepository
                .GetByIdAndTenantAsync(
                    requestId,
                    tenantId);

            if (request == null)
            {
                throw new UnauthorizedAccessException(
                    "Viewing request not found or access denied.");
            }

            // Step D: Only requests waiting for
            // reschedule confirmation can be accepted.
            if (request.Status !=
                ViewingRequestStatus.RescheduleRequested)
            {
                throw new InvalidOperationException(
                    "This request is not awaiting " +
                    "reschedule confirmation.");
            }

            // Step E: Verify the owner proposed a time.
            if (!request.ProposedDateTime.HasValue)
            {
                throw new InvalidOperationException(
                    "No alternative viewing time was proposed.");
            }

            // Step F: Proposed time must still be in the future.
            DateTime now = DateTime.UtcNow;

            if (request.ProposedDateTime.Value <= now)
            {
                throw new InvalidOperationException(
                    "The proposed viewing time has already passed.");
            }

            // Step G: Accept the proposed time.
            request.Status = ViewingRequestStatus.Accepted;

            // Keep the original requested time
            // and the owner's proposed time for reference.
            // The accepted viewing time is ProposedDateTime.

            // Step H: Update audit timestamp.
            request.UpdatedAt = now;

            // Step I: Save changes to SQL Server.
            await _viewingRequestRepository
                .SaveChangesAsync();
        }



        // 10. Tenant cancels their own viewing request
        public async Task CancelAsync(
            int requestId,
            string tenantId)
        {
            // Step A: Validate logged-in Tenant ID.
            ValidateUserId(tenantId);

            // Step B: Validate viewing request ID.
            if (requestId <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(requestId),
                    "A valid viewing request ID is required.");
            }

            // Step C: Retrieve the request
            // with Tenant ownership verification.
            var request = await _viewingRequestRepository
                .GetByIdAndTenantAsync(
                    requestId,
                    tenantId);

            if (request == null)
            {
                throw new UnauthorizedAccessException(
                    "Viewing request not found or access denied.");
            }

            // Step D: Only Pending, RescheduleRequested
            // or Accepted requests can be cancelled.
            bool canCancel =
                request.Status == ViewingRequestStatus.Pending ||
                request.Status == ViewingRequestStatus.RescheduleRequested ||
                request.Status == ViewingRequestStatus.Accepted;

            if (!canCancel)
            {
                throw new InvalidOperationException(
                    "This viewing request cannot be cancelled " +
                    "in its current status.");
            }

            // Step E: Change status to Cancelled.
            request.Status = ViewingRequestStatus.Cancelled;

            // Step F: Preserve original requested time,
            // proposed time and owner response
            // for viewing request history.

            // Step G: Update audit timestamp.
            request.UpdatedAt = DateTime.UtcNow;

            // Step H: Save changes to SQL Server.
            await _viewingRequestRepository
                .SaveChangesAsync();
        }



        // 11. PropertyOwner marks an accepted viewing as completed
        public async Task CompleteAsync(
            int requestId,
            string ownerId)
        {
            // Step A: Validate logged-in PropertyOwner ID.
            ValidateUserId(ownerId);

            // Step B: Validate viewing request ID.
            if (requestId <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(requestId),
                    "A valid viewing request ID is required.");
            }

            // Step C: Retrieve request with owner verification.
            var request = await _viewingRequestRepository
                .GetByIdAndOwnerAsync(
                    requestId,
                    ownerId);

            if (request == null)
            {
                throw new UnauthorizedAccessException(
                    "Viewing request not found or access denied.");
            }

            // Step D: Only accepted requests can be completed.
            if (request.Status != ViewingRequestStatus.Accepted)
            {
                throw new InvalidOperationException(
                    "Only accepted viewing requests can be completed.");
            }

            // Step E: Determine the confirmed viewing time.
            // If Tenant accepted the owner's proposed time,
            // use ProposedDateTime.
            // Otherwise, use the original RequestedDateTime.
            DateTime confirmedViewingTimeUtc =
                request.ProposedDateTime
                ?? request.RequestedDateTime;

            // Step F: The scheduled viewing time must have arrived.
            DateTime now = DateTime.UtcNow;

            if (confirmedViewingTimeUtc > now)
            {
                throw new InvalidOperationException(
                    "The scheduled viewing time has not arrived yet.");
            }

            // Step G: Mark the viewing as completed.
            request.Status = ViewingRequestStatus.Completed;

            // Step H: Preserve all original request details,
            // including requested time, proposed time,
            // tenant message and owner response.

            // Step I: Update audit timestamp.
            request.UpdatedAt = now;

            // Step J: Save changes to SQL Server.
            await _viewingRequestRepository
                .SaveChangesAsync();
        }


        // Shared helper: validate user ID
        private static void ValidateUserId(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentException(
                    "A valid user ID is required.",
                    nameof(userId));
            }
        }
    }
}
