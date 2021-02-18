using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
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
        public ActionResult<QueryResult<BaseItemDto>> GetInstantMixFromSong(
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
                if (!RequestHelpers.AssertCanUpdateUser(_authContext, HttpContext.Request, userId.Value, false))
                {
                    return StatusCode(StatusCodes.Status403Forbidden, "User does not have permission for this action.");
                }
            }

            var item = _libraryManager.GetItemById(id);
            var user = userId.HasValue && !userId.Equals(Guid.Empty)
                ? _userManager.GetUserById(userId.Value)
                : null;
            var dtoOptions = new DtoOptions { Fields = fields }
                .AddClientFields(Request)
                .AddAdditionalDtoOptions(enableImages, enableUserData, imageTypeLimit, enableImageTypes!);
            var items = _musicManager.GetInstantMixFromItem(item, user, dtoOptions);
            return GetResult(items, user, limit, dtoOptions);
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
        [HttpGet("Albums/{id}/InstantMix")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<QueryResult<BaseItemDto>> GetInstantMixFromAlbum(
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
                if (!RequestHelpers.AssertCanUpdateUser(_authContext, HttpContext.Request, userId.Value, false))
                {
                    return StatusCode(StatusCodes.Status403Forbidden, "User does not have permission for this action.");
                }
            }

            var album = _libraryManager.GetItemById(id);
            var user = userId.HasValue && !userId.Equals(Guid.Empty)
                ? _userManager.GetUserById(userId.Value)
                : null;
            var dtoOptions = new DtoOptions { Fields = fields }
                .AddClientFields(Request)
                .AddAdditionalDtoOptions(enableImages, enableUserData, imageTypeLimit, enableImageTypes!);
            var items = _musicManager.GetInstantMixFromItem(album, user, dtoOptions);
            return GetResult(items, user, limit, dtoOptions);
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
        [HttpGet("Playlists/{id}/InstantMix")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<QueryResult<BaseItemDto>> GetInstantMixFromPlaylist(
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
                if (!RequestHelpers.AssertCanUpdateUser(_authContext, HttpContext.Request, userId.Value, false))
                {
                    return StatusCode(StatusCodes.Status403Forbidden, "User does not have permission for this action.");
                }
            }

            var playlist = (Playlist)_libraryManager.GetItemById(id);
            var user = userId.HasValue && !userId.Equals(Guid.Empty)
                ? _userManager.GetUserById(userId.Value)
                : null;
            var dtoOptions = new DtoOptions { Fields = fields }
                .AddClientFields(Request)
                .AddAdditionalDtoOptions(enableImages, enableUserData, imageTypeLimit, enableImageTypes!);
            var items = _musicManager.GetInstantMixFromItem(playlist, user, dtoOptions);
            return GetResult(items, user, limit, dtoOptions);
        }

        /// <summary>
        /// Creates an instant playlist based on a given song.
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
        public ActionResult<QueryResult<BaseItemDto>> GetInstantMixFromMusicGenre(
            [FromRoute, Required] string name,
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
                if (!RequestHelpers.AssertCanUpdateUser(_authContext, HttpContext.Request, userId.Value, false))
                {
                    return StatusCode(StatusCodes.Status403Forbidden, "User does not have permission for this action.");
                }
            }

            var user = userId.HasValue && !userId.Equals(Guid.Empty)
                ? _userManager.GetUserById(userId.Value)
                : null;
            var dtoOptions = new DtoOptions { Fields = fields }
                .AddClientFields(Request)
                .AddAdditionalDtoOptions(enableImages, enableUserData, imageTypeLimit, enableImageTypes!);
            var items = _musicManager.GetInstantMixFromGenres(new[] { name }, user, dtoOptions);
            return GetResult(items, user, limit, dtoOptions);
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
        [HttpGet("Artists/{id}/InstantMix")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<QueryResult<BaseItemDto>> GetInstantMixFromArtists(
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
                if (!RequestHelpers.AssertCanUpdateUser(_authContext, HttpContext.Request, userId.Value, false))
                {
                    return StatusCode(StatusCodes.Status403Forbidden, "User does not have permission for this action.");
                }
            }

            var item = _libraryManager.GetItemById(id);
            var user = userId.HasValue && !userId.Equals(Guid.Empty)
                ? _userManager.GetUserById(userId.Value)
                : null;
            var dtoOptions = new DtoOptions { Fields = fields }
                .AddClientFields(Request)
                .AddAdditionalDtoOptions(enableImages, enableUserData, imageTypeLimit, enableImageTypes!);
            var items = _musicManager.GetInstantMixFromItem(item, user, dtoOptions);
            return GetResult(items, user, limit, dtoOptions);
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
        [HttpGet("MusicGenres/{id}/InstantMix")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<QueryResult<BaseItemDto>> GetInstantMixFromMusicGenres(
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
                if (!RequestHelpers.AssertCanUpdateUser(_authContext, HttpContext.Request, userId.Value, false))
                {
                    return StatusCode(StatusCodes.Status403Forbidden, "User does not have permission for this action.");
                }
            }

            var item = _libraryManager.GetItemById(id);
            var user = userId.HasValue && !userId.Equals(Guid.Empty)
                ? _userManager.GetUserById(userId.Value)
                : null;
            var dtoOptions = new DtoOptions { Fields = fields }
                .AddClientFields(Request)
                .AddAdditionalDtoOptions(enableImages, enableUserData, imageTypeLimit, enableImageTypes!);
            var items = _musicManager.GetInstantMixFromItem(item, user, dtoOptions);
            return GetResult(items, user, limit, dtoOptions);
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
        [HttpGet("Items/{id}/InstantMix")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<QueryResult<BaseItemDto>> GetInstantMixFromItem(
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
                if (!RequestHelpers.AssertCanUpdateUser(_authContext, HttpContext.Request, userId.Value, false))
                {
                    return StatusCode(StatusCodes.Status403Forbidden, "User does not have permission for this action.");
                }
            }

            var item = _libraryManager.GetItemById(id);
            var user = userId.HasValue && !userId.Equals(Guid.Empty)
                ? _userManager.GetUserById(userId.Value)
                : null;
            var dtoOptions = new DtoOptions { Fields = fields }
                .AddClientFields(Request)
                .AddAdditionalDtoOptions(enableImages, enableUserData, imageTypeLimit, enableImageTypes!);
            var items = _musicManager.GetInstantMixFromItem(item, user, dtoOptions);
            return GetResult(items, user, limit, dtoOptions);
        }

        private QueryResult<BaseItemDto> GetResult(List<BaseItem> items, User? user, int? limit, DtoOptions dtoOptions)
        {
            var list = items;

            var result = new QueryResult<BaseItemDto>
            {
                TotalRecordCount = list.Count
            };

            if (limit.HasValue && limit < list.Count)
            {
                list = list.GetRange(0, limit.Value);
            }

            var returnList = _dtoService.GetBaseItemDtos(list, dtoOptions, user);

            result.Items = returnList;

            return result;
        }
    }
}
