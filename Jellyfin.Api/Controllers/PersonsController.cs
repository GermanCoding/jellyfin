using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Jellyfin.Api.Constants;
using Jellyfin.Api.Extensions;
using Jellyfin.Api.Helpers;
using Jellyfin.Api.ModelBinders;
using Jellyfin.Data.Entities;
using MediaBrowser.Controller.Dto;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Net;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Querying;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Jellyfin.Api.Controllers
{
    /// <summary>
    /// Persons controller.
    /// </summary>
    [Authorize(Policy = Policies.DefaultAuthorization)]
    public class PersonsController : BaseJellyfinApiController
    {
        private readonly ILibraryManager _libraryManager;
        private readonly IDtoService _dtoService;
        private readonly IUserManager _userManager;
        private readonly IUserDataManager _userDataManager;
        private readonly IAuthorizationContext _authContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="PersonsController"/> class.
        /// </summary>
        /// <param name="libraryManager">Instance of the <see cref="ILibraryManager"/> interface.</param>
        /// <param name="dtoService">Instance of the <see cref="IDtoService"/> interface.</param>
        /// <param name="userManager">Instance of the <see cref="IUserManager"/> interface.</param>
        /// <param name="userDataManager">Instance of the <see cref="IUserDataManager"/> interface.</param>
        /// <param name="authContext">Instance of the <see cref="IAuthorizationContext"/> interface.</param>
        public PersonsController(
            ILibraryManager libraryManager,
            IDtoService dtoService,
            IUserManager userManager,
            IUserDataManager userDataManager,
            IAuthorizationContext authContext)
        {
            _libraryManager = libraryManager;
            _dtoService = dtoService;
            _userManager = userManager;
            _userDataManager = userDataManager;
            _authContext = authContext;
        }

        /// <summary>
        /// Gets all persons.
        /// </summary>
        /// <param name="limit">Optional. The maximum number of records to return.</param>
        /// <param name="searchTerm">The search term.</param>
        /// <param name="fields">Optional. Specify additional fields of information to return in the output.</param>
        /// <param name="filters">Optional. Specify additional filters to apply.</param>
        /// <param name="isFavorite">Optional filter by items that are marked as favorite, or not. userId is required.</param>
        /// <param name="enableUserData">Optional, include user data.</param>
        /// <param name="imageTypeLimit">Optional, the max number of images to return, per image type.</param>
        /// <param name="enableImageTypes">Optional. The image types to include in the output.</param>
        /// <param name="excludePersonTypes">Optional. If specified results will be filtered to exclude those containing the specified PersonType. Allows multiple, comma-delimited.</param>
        /// <param name="personTypes">Optional. If specified results will be filtered to include only those containing the specified PersonType. Allows multiple, comma-delimited.</param>
        /// <param name="appearsInItemId">Optional. If specified, person results will be filtered on items related to said persons.</param>
        /// <param name="userId">User id.</param>
        /// <param name="enableImages">Optional, include image information in output.</param>
        /// <response code="200">Persons returned.</response>
        /// <returns>An <see cref="OkResult"/> containing the queryresult of persons.</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<QueryResult<BaseItemDto>>> GetPersonsAsync(
            [FromQuery] int? limit,
            [FromQuery] string? searchTerm,
            [FromQuery, ModelBinder(typeof(CommaDelimitedArrayModelBinder))] ItemFields[] fields,
            [FromQuery, ModelBinder(typeof(CommaDelimitedArrayModelBinder))] ItemFilter[] filters,
            [FromQuery] bool? isFavorite,
            [FromQuery] bool? enableUserData,
            [FromQuery] int? imageTypeLimit,
            [FromQuery, ModelBinder(typeof(CommaDelimitedArrayModelBinder))] ImageType[] enableImageTypes,
            [FromQuery, ModelBinder(typeof(CommaDelimitedArrayModelBinder))] string[] excludePersonTypes,
            [FromQuery, ModelBinder(typeof(CommaDelimitedArrayModelBinder))] string[] personTypes,
            [FromQuery] Guid? appearsInItemId,
            [FromQuery] Guid? userId,
            [FromQuery] bool? enableImages = true)
        {
            if (userId.HasValue)
            {
                if (!await RequestHelpers.AssertCanUpdateUser(_authContext, HttpContext.Request, userId.Value, false).ConfigureAwait(false))
                {
                    return StatusCode(StatusCodes.Status403Forbidden, "User does not have permission for this action.");
                }
            }

            var dtoOptions = new DtoOptions { Fields = fields }
                .AddClientFields(Request)
                .AddAdditionalDtoOptions(enableImages, enableUserData, imageTypeLimit, enableImageTypes);

            User? user = userId is null || userId.Value.Equals(default)
                ? null
                : _userManager.GetUserById(userId.Value);

            var isFavoriteInFilters = filters.Any(f => f == ItemFilter.IsFavorite);
            var peopleItems = _libraryManager.GetPeopleItems(new InternalPeopleQuery(
                personTypes,
                excludePersonTypes)
            {
                NameContains = searchTerm,
                User = user,
                IsFavorite = !isFavorite.HasValue && isFavoriteInFilters ? true : isFavorite,
                AppearsInItemId = appearsInItemId ?? Guid.Empty,
                Limit = limit ?? 0
            });

            return new QueryResult<BaseItemDto>(
                peopleItems
                .Select(person => _dtoService.GetItemByNameDto(person, dtoOptions, null, user))
                .ToArray());
        }

        /// <summary>
        /// Get person by name.
        /// </summary>
        /// <param name="name">Person name.</param>
        /// <param name="userId">Optional. Filter by user id, and attach user data.</param>
        /// <response code="200">Person returned.</response>
        /// <response code="404">Person not found.</response>
        /// <returns>An <see cref="OkResult"/> containing the person on success,
        /// or a <see cref="NotFoundResult"/> if person not found.</returns>
        [HttpGet("{name}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<BaseItemDto>> GetPersonAsync([FromRoute, Required] string name, [FromQuery] Guid? userId)
        {
            if (userId.HasValue)
            {
                if (!await RequestHelpers.AssertCanUpdateUser(_authContext, HttpContext.Request, userId.Value, false).ConfigureAwait(false))
                {
                    return StatusCode(StatusCodes.Status403Forbidden, "User does not have permission for this action.");
                }
            }

            var dtoOptions = new DtoOptions()
                .AddClientFields(Request);

            var item = _libraryManager.GetPerson(name);
            if (item == null)
            {
                return NotFound();
            }

            if (userId.HasValue && !userId.Value.Equals(default))
            {
                var user = _userManager.GetUserById(userId.Value);
                return _dtoService.GetBaseItemDto(item, dtoOptions, user);
            }

            return _dtoService.GetBaseItemDto(item, dtoOptions);
        }
    }
}
