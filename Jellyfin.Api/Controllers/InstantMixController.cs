using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
using MediaBrowser.Controller.Playlists;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Querying;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Jellyfin.Api.Controllers
{
    /// <summary>
    /// The instant mix controller.
    /// </summary>
    [Route("")]
    [Authorize(Policy = Policies.DefaultAuthorization)]
    public class InstantMixController : BaseJellyfinApiController
    {
        private readonly IUserManager _userManager;
        private readonly IDtoService _dtoService;
        private readonly ILibraryManager _libraryManager;
        private readonly IMusicManager _musicManager;
        private readonly IAuthorizationContext _authContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="InstantMixController"/> class.
        /// </summary>
        /// <param name="userManager">Instance of the <see cref="IUserManager"/> interface.</param>
        /// <param name="dtoService">Instance of the <see cref="IDtoService"/> interface.</param>
        /// <param name="musicManager">Instance of the <see cref="IMusicManager"/> interface.</param>
        /// <param name="libraryManager">Instance of the <see cref="ILibraryManager"/> interface.</param>
        /// <param name="authContext">Instance of the <see cref="IAuthorizationContext"/> interface.</param>
        public InstantMixController(
            IUserManager userManager,
            IDtoService dtoService,
            IMusicManager musicManager,
            ILibraryManager libraryManager,
            IAuthorizationContext authContext)
        {
            _userManager = userManager;
            _dtoService = dtoService;
            _musicManager = musicManager;
            _libraryManager = libraryManager;
            _authContext = authContext;
        }

        /// <summary>
        /// Creates an instant playlist based on a given song.
        /// </summary>
        /// <param name="id">The item id.</param>
        /// <param name="userId">Optional. Filter by user id, and attach user data.</param>
        /// <param name="limit">Optional. The maximum number of records to return.</param>
        /// <param name="fields">Optional. Specify additional fields of information to return in the output.</param>
        /// <param name="enableImages">Optional. Include image information in output.</param>
        /// <param name="enableUserData">Optional. Include user data.</param>
        /// <param name="imageTypeLimit">Optional. The max number of images to return, per image type.</param>
        /// <param name="enableImageTypes">Optional. The image types to include in the output.</param>
        /// <response code="200">Instant playlist returned.</response>
        /// <returns>A <see cref="QueryResult{BaseItemDto}"/> with the playlist items.</returns>
        [HttpGet("Songs/{id}/InstantMix")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<QueryResult<BaseItemDto>>> GetInstantMixFromSongAsync(
            [FromRoute, Required] Guid id,
            [FromQuery] Guid? userId,
            [FromQuery] int? limit,
            [FromQuery, ModelBinder(typeof(CommaDelimitedArrayModelBinder))] ItemFields[] fields,
            [FromQuery] bool? enableImages,
            [FromQuery] bool? enableUserData,
            [FromQuery] int? imageTypeLimit,
            [FromQuery, ModelBinder(typeof(CommaDelimitedArrayModelBinder))] ImageType[] enableImageTypes)
        {
            if (userId.HasValue)
            {
                if (!await RequestHelpers.AssertCanUpdateUser(_authContext, HttpContext.Request, userId.Value, false).ConfigureAwait(false))
                {
                    return StatusCode(StatusCodes.Status403Forbidden, "User does not have permission for this action.");
                }
            }

            var item = _libraryManager.GetItemById(id);
            var user = userId is null || userId.Value.Equals(default)
                ? null
                : _userManager.GetUserById(userId.Value);
            var dtoOptions = new DtoOptions { Fields = fields }
                .AddClientFields(Request)
                .AddAdditionalDtoOptions(enableImages, enableUserData, imageTypeLimit, enableImageTypes);
            var items = _musicManager.GetInstantMixFromItem(item, user, dtoOptions);
            return GetResult(items, user, limit, dtoOptions);
        }

        /// <summary>
        /// Creates an instant playlist based on a given album.
        /// </summary>
        /// <param name="id">The item id.</param>
        /// <param name="userId">Optional. Filter by user id, and attach user data.</param>
        /// <param name="limit">Optional. The maximum number of records to return.</param>
        /// <param name="fields">Optional. Specify additional fields of information to return in the output.</param>
        /// <param name="enableImages">Optional. Include image information in output.</param>
        /// <param name="enableUserData">Optional. Include user data.</param>
        /// <param name="imageTypeLimit">Optional. The max number of images to return, per image type.</param>
        /// <param name="enableImageTypes">Optional. The image types to include in the output.</param>
        /// <response code="200">Instant playlist returned.</response>
        /// <returns>A <see cref="QueryResult{BaseItemDto}"/> with the playlist items.</returns>
        [HttpGet("Albums/{id}/InstantMix")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<QueryResult<BaseItemDto>>> GetInstantMixFromAlbumAsync(
            [FromRoute, Required] Guid id,
            [FromQuery] Guid? userId,
            [FromQuery] int? limit,
            [FromQuery, ModelBinder(typeof(CommaDelimitedArrayModelBinder))] ItemFields[] fields,
            [FromQuery] bool? enableImages,
            [FromQuery] bool? enableUserData,
            [FromQuery] int? imageTypeLimit,
            [FromQuery, ModelBinder(typeof(CommaDelimitedArrayModelBinder))] ImageType[] enableImageTypes)
        {
            if (userId.HasValue)
            {
                if (!await RequestHelpers.AssertCanUpdateUser(_authContext, HttpContext.Request, userId.Value, false).ConfigureAwait(false))
                {
                    return StatusCode(StatusCodes.Status403Forbidden, "User does not have permission for this action.");
                }
            }

            var album = _libraryManager.GetItemById(id);
            var user = userId is null || userId.Value.Equals(default)
                ? null
                : _userManager.GetUserById(userId.Value);
            var dtoOptions = new DtoOptions { Fields = fields }
                .AddClientFields(Request)
                .AddAdditionalDtoOptions(enableImages, enableUserData, imageTypeLimit, enableImageTypes);
            var items = _musicManager.GetInstantMixFromItem(album, user, dtoOptions);
            return GetResult(items, user, limit, dtoOptions);
        }

        /// <summary>
        /// Creates an instant playlist based on a given playlist.
        /// </summary>
        /// <param name="id">The item id.</param>
        /// <param name="userId">Optional. Filter by user id, and attach user data.</param>
        /// <param name="limit">Optional. The maximum number of records to return.</param>
        /// <param name="fields">Optional. Specify additional fields of information to return in the output.</param>
        /// <param name="enableImages">Optional. Include image information in output.</param>
        /// <param name="enableUserData">Optional. Include user data.</param>
        /// <param name="imageTypeLimit">Optional. The max number of images to return, per image type.</param>
        /// <param name="enableImageTypes">Optional. The image types to include in the output.</param>
        /// <response code="200">Instant playlist returned.</response>
        /// <returns>A <see cref="QueryResult{BaseItemDto}"/> with the playlist items.</returns>
        [HttpGet("Playlists/{id}/InstantMix")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<QueryResult<BaseItemDto>>> GetInstantMixFromPlaylistAsync(
            [FromRoute, Required] Guid id,
            [FromQuery] Guid? userId,
            [FromQuery] int? limit,
            [FromQuery, ModelBinder(typeof(CommaDelimitedArrayModelBinder))] ItemFields[] fields,
            [FromQuery] bool? enableImages,
            [FromQuery] bool? enableUserData,
            [FromQuery] int? imageTypeLimit,
            [FromQuery, ModelBinder(typeof(CommaDelimitedArrayModelBinder))] ImageType[] enableImageTypes)
        {
            if (userId.HasValue)
            {
                if (!await RequestHelpers.AssertCanUpdateUser(_authContext, HttpContext.Request, userId.Value, false).ConfigureAwait(false))
                {
                    return StatusCode(StatusCodes.Status403Forbidden, "User does not have permission for this action.");
                }
            }

            var playlist = (Playlist)_libraryManager.GetItemById(id);
            var user = userId is null || userId.Value.Equals(default)
                ? null
                : _userManager.GetUserById(userId.Value);
            var dtoOptions = new DtoOptions { Fields = fields }
                .AddClientFields(Request)
                .AddAdditionalDtoOptions(enableImages, enableUserData, imageTypeLimit, enableImageTypes);
            var items = _musicManager.GetInstantMixFromItem(playlist, user, dtoOptions);
            return GetResult(items, user, limit, dtoOptions);
        }

        /// <summary>
        /// Creates an instant playlist based on a given genre.
        /// </summary>
        /// <param name="name">The genre name.</param>
        /// <param name="userId">Optional. Filter by user id, and attach user data.</param>
        /// <param name="limit">Optional. The maximum number of records to return.</param>
        /// <param name="fields">Optional. Specify additional fields of information to return in the output.</param>
        /// <param name="enableImages">Optional. Include image information in output.</param>
        /// <param name="enableUserData">Optional. Include user data.</param>
        /// <param name="imageTypeLimit">Optional. The max number of images to return, per image type.</param>
        /// <param name="enableImageTypes">Optional. The image types to include in the output.</param>
        /// <response code="200">Instant playlist returned.</response>
        /// <returns>A <see cref="QueryResult{BaseItemDto}"/> with the playlist items.</returns>
        [HttpGet("MusicGenres/{name}/InstantMix")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<QueryResult<BaseItemDto>>> GetInstantMixFromMusicGenreByNameAsync(
            [FromRoute, Required] string name,
            [FromQuery] Guid? userId,
            [FromQuery] int? limit,
            [FromQuery, ModelBinder(typeof(CommaDelimitedArrayModelBinder))] ItemFields[] fields,
            [FromQuery] bool? enableImages,
            [FromQuery] bool? enableUserData,
            [FromQuery] int? imageTypeLimit,
            [FromQuery, ModelBinder(typeof(CommaDelimitedArrayModelBinder))] ImageType[] enableImageTypes)
        {
            var user = userId is null || userId.Value.Equals(default)
                ? null
                : _userManager.GetUserById(userId.Value);

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
            var items = _musicManager.GetInstantMixFromGenres(new[] { name }, user, dtoOptions);
            return GetResult(items, user, limit, dtoOptions);
        }

        /// <summary>
        /// Creates an instant playlist based on a given artist.
        /// </summary>
        /// <param name="id">The item id.</param>
        /// <param name="userId">Optional. Filter by user id, and attach user data.</param>
        /// <param name="limit">Optional. The maximum number of records to return.</param>
        /// <param name="fields">Optional. Specify additional fields of information to return in the output.</param>
        /// <param name="enableImages">Optional. Include image information in output.</param>
        /// <param name="enableUserData">Optional. Include user data.</param>
        /// <param name="imageTypeLimit">Optional. The max number of images to return, per image type.</param>
        /// <param name="enableImageTypes">Optional. The image types to include in the output.</param>
        /// <response code="200">Instant playlist returned.</response>
        /// <returns>A <see cref="QueryResult{BaseItemDto}"/> with the playlist items.</returns>
        [HttpGet("Artists/{id}/InstantMix")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<QueryResult<BaseItemDto>>> GetInstantMixFromArtistsAsync(
            [FromRoute, Required] Guid id,
            [FromQuery] Guid? userId,
            [FromQuery] int? limit,
            [FromQuery, ModelBinder(typeof(CommaDelimitedArrayModelBinder))] ItemFields[] fields,
            [FromQuery] bool? enableImages,
            [FromQuery] bool? enableUserData,
            [FromQuery] int? imageTypeLimit,
            [FromQuery, ModelBinder(typeof(CommaDelimitedArrayModelBinder))] ImageType[] enableImageTypes)
        {
            if (userId.HasValue)
            {
                if (!await RequestHelpers.AssertCanUpdateUser(_authContext, HttpContext.Request, userId.Value, false).ConfigureAwait(false))
                {
                    return StatusCode(StatusCodes.Status403Forbidden, "User does not have permission for this action.");
                }
            }

            var item = _libraryManager.GetItemById(id);
            var user = userId is null || userId.Value.Equals(default)
                ? null
                : _userManager.GetUserById(userId.Value);
            var dtoOptions = new DtoOptions { Fields = fields }
                .AddClientFields(Request)
                .AddAdditionalDtoOptions(enableImages, enableUserData, imageTypeLimit, enableImageTypes);
            var items = _musicManager.GetInstantMixFromItem(item, user, dtoOptions);
            return GetResult(items, user, limit, dtoOptions);
        }

        /// <summary>
        /// Creates an instant playlist based on a given item.
        /// </summary>
        /// <param name="id">The item id.</param>
        /// <param name="userId">Optional. Filter by user id, and attach user data.</param>
        /// <param name="limit">Optional. The maximum number of records to return.</param>
        /// <param name="fields">Optional. Specify additional fields of information to return in the output.</param>
        /// <param name="enableImages">Optional. Include image information in output.</param>
        /// <param name="enableUserData">Optional. Include user data.</param>
        /// <param name="imageTypeLimit">Optional. The max number of images to return, per image type.</param>
        /// <param name="enableImageTypes">Optional. The image types to include in the output.</param>
        /// <response code="200">Instant playlist returned.</response>
        /// <returns>A <see cref="QueryResult{BaseItemDto}"/> with the playlist items.</returns>
        [HttpGet("Items/{id}/InstantMix")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<QueryResult<BaseItemDto>>> GetInstantMixFromItemAsync(
            [FromRoute, Required] Guid id,
            [FromQuery] Guid? userId,
            [FromQuery] int? limit,
            [FromQuery, ModelBinder(typeof(CommaDelimitedArrayModelBinder))] ItemFields[] fields,
            [FromQuery] bool? enableImages,
            [FromQuery] bool? enableUserData,
            [FromQuery] int? imageTypeLimit,
            [FromQuery, ModelBinder(typeof(CommaDelimitedArrayModelBinder))] ImageType[] enableImageTypes)
        {
            if (userId.HasValue)
            {
                if (!await RequestHelpers.AssertCanUpdateUser(_authContext, HttpContext.Request, userId.Value, false).ConfigureAwait(false))
                {
                    return StatusCode(StatusCodes.Status403Forbidden, "User does not have permission for this action.");
                }
            }

            var item = _libraryManager.GetItemById(id);
            var user = userId is null || userId.Value.Equals(default)
                ? null
                : _userManager.GetUserById(userId.Value);
            var dtoOptions = new DtoOptions { Fields = fields }
                .AddClientFields(Request)
                .AddAdditionalDtoOptions(enableImages, enableUserData, imageTypeLimit, enableImageTypes);
            var items = _musicManager.GetInstantMixFromItem(item, user, dtoOptions);
            return GetResult(items, user, limit, dtoOptions);
        }

        /// <summary>
        /// Creates an instant playlist based on a given artist.
        /// </summary>
        /// <param name="id">The item id.</param>
        /// <param name="userId">Optional. Filter by user id, and attach user data.</param>
        /// <param name="limit">Optional. The maximum number of records to return.</param>
        /// <param name="fields">Optional. Specify additional fields of information to return in the output.</param>
        /// <param name="enableImages">Optional. Include image information in output.</param>
        /// <param name="enableUserData">Optional. Include user data.</param>
        /// <param name="imageTypeLimit">Optional. The max number of images to return, per image type.</param>
        /// <param name="enableImageTypes">Optional. The image types to include in the output.</param>
        /// <response code="200">Instant playlist returned.</response>
        /// <returns>A <see cref="QueryResult{BaseItemDto}"/> with the playlist items.</returns>
        [HttpGet("Artists/InstantMix")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Obsolete("Use GetInstantMixFromArtists")]
        public async Task<ActionResult<QueryResult<BaseItemDto>>> GetInstantMixFromArtists2Async(
            [FromQuery, Required] Guid id,
            [FromQuery] Guid? userId,
            [FromQuery] int? limit,
            [FromQuery, ModelBinder(typeof(CommaDelimitedArrayModelBinder))] ItemFields[] fields,
            [FromQuery] bool? enableImages,
            [FromQuery] bool? enableUserData,
            [FromQuery] int? imageTypeLimit,
            [FromQuery, ModelBinder(typeof(CommaDelimitedArrayModelBinder))] ImageType[] enableImageTypes)
        {
            return await GetInstantMixFromArtistsAsync(
                id,
                userId,
                limit,
                fields,
                enableImages,
                enableUserData,
                imageTypeLimit,
                enableImageTypes).ConfigureAwait(false);
        }

        /// <summary>
        /// Creates an instant playlist based on a given genre.
        /// </summary>
        /// <param name="id">The item id.</param>
        /// <param name="userId">Optional. Filter by user id, and attach user data.</param>
        /// <param name="limit">Optional. The maximum number of records to return.</param>
        /// <param name="fields">Optional. Specify additional fields of information to return in the output.</param>
        /// <param name="enableImages">Optional. Include image information in output.</param>
        /// <param name="enableUserData">Optional. Include user data.</param>
        /// <param name="imageTypeLimit">Optional. The max number of images to return, per image type.</param>
        /// <param name="enableImageTypes">Optional. The image types to include in the output.</param>
        /// <response code="200">Instant playlist returned.</response>
        /// <returns>A <see cref="QueryResult{BaseItemDto}"/> with the playlist items.</returns>
        [HttpGet("MusicGenres/InstantMix")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<QueryResult<BaseItemDto>> GetInstantMixFromMusicGenreById(
            [FromQuery, Required] Guid id,
            [FromQuery] Guid? userId,
            [FromQuery] int? limit,
            [FromQuery, ModelBinder(typeof(CommaDelimitedArrayModelBinder))] ItemFields[] fields,
            [FromQuery] bool? enableImages,
            [FromQuery] bool? enableUserData,
            [FromQuery] int? imageTypeLimit,
            [FromQuery, ModelBinder(typeof(CommaDelimitedArrayModelBinder))] ImageType[] enableImageTypes)
        {
            var item = _libraryManager.GetItemById(id);
            var user = userId is null || userId.Value.Equals(default)
                ? null
                : _userManager.GetUserById(userId.Value);
            var dtoOptions = new DtoOptions { Fields = fields }
                .AddClientFields(Request)
                .AddAdditionalDtoOptions(enableImages, enableUserData, imageTypeLimit, enableImageTypes);
            var items = _musicManager.GetInstantMixFromItem(item, user, dtoOptions);
            return GetResult(items, user, limit, dtoOptions);
        }

        private QueryResult<BaseItemDto> GetResult(List<BaseItem> items, User? user, int? limit, DtoOptions dtoOptions)
        {
            var list = items;

            var totalCount = list.Count;

            if (limit.HasValue && limit < list.Count)
            {
                list = list.GetRange(0, limit.Value);
            }

            var returnList = _dtoService.GetBaseItemDtos(list, dtoOptions, user);

            var result = new QueryResult<BaseItemDto>(
                0,
                totalCount,
                returnList);

            return result;
        }
    }
}
