using ABBELForgeService.Domain.Entities;
using ABBELForgeService.Domain.Entities.Response;
using ABBELForgeService.Domain.Services.Class;
using ABBELForgeService.Domain.Services.Interface;
using ABBELForgeService.Domain.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography.Xml;

namespace ABBELForgeService.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ForgeController : ControllerBase
    {
        private readonly IBucketService _bucketService;
        private readonly ISignedUrlService _signedUrlService;
        private readonly IUploadFileService _uploadFileService;
        private readonly ITranslationService _translationService;
        private readonly IMetadataService _metadataService;
        private ILogger<ForgeController> _logger;

        public ForgeController(IBucketService bucketService, ISignedUrlService signedUrlService,
            IUploadFileService uploadFileService, ITranslationService translationService,
            IMetadataService metadataService, ILogger<ForgeController> logger)
        {
            _bucketService = bucketService;
            _signedUrlService = signedUrlService;
            _uploadFileService = uploadFileService;
            _translationService = translationService;
            _metadataService = metadataService;
            _logger = logger;
        }

        [HttpGet("getMetadata")]
        public async Task<IActionResult> getMetadata(string urn)
        {
            _logger.LogInformation("GetMetadata");
            var metadata = await _metadataService.GetMetadata(urn);
            return Ok(metadata);
        }

        [HttpGet("getMetadataProperties")]
        public async Task<IActionResult> getMetadata(string urn, string id)
        {
            var metadata = await _metadataService.GetMetadataProperties(urn, id);
            return Ok(metadata);
        }

        [HttpGet("getMetadataRVTMasterViewProperties")]
        public async Task<IActionResult> getMetadataRVTMasterViewProperties(string urn)
        {
            var metadataLst = await _metadataService.GetMetadata(urn);
            var metadataMasterView = metadataLst.data.metadata.FirstOrDefault(x => x.isMasterView);

            var properties = await _metadataService.GetMetadataRVTMasterViewProperties(urn, metadataMasterView.guid);

            Console.WriteLine(properties.data.collection.Count());
            properties.data.collection.RemoveAll(x => x.properties.Dimensions?.Area == null);
            properties.data.collection.RemoveAll(x => x.properties.Dimensions?.Perimeter == null);
            properties.data.collection.RemoveAll(x => x.properties.Dimensions?.Volume == null);
            properties.data.collection.RemoveAll(x => x.properties.Dimensions == null);
            properties.data.collection.RemoveAll(x => x.properties.IdentityData?.Name == null);
            Console.WriteLine(properties.data.collection.Count());

            var rooms = new List<Rooms>();

            foreach (var item in properties.data.collection)
            {
                var room = new Rooms()
                {
                    Area = item.properties.Dimensions.Area,
                    Name = item.properties.IdentityData.Name,
                    Perimeter = item.properties.Dimensions.Perimeter,
                    Volume = item.properties.Dimensions.Volume
                };
                rooms.Add(room);
            }

            var details = new FullProperties()
            {
                Rooms = rooms
            };
            return Ok(details);
        }
        
        [HttpPost("fullUploadRevit")]
        public async Task<IActionResult> fullUploadRevit(IFormFile file)
        {
            var bucketName = "bucket_poc";
            var svf = "svf";
            var bytes = await file.GetBytes();
            var bucketStr = bucketName;

            await _bucketService.CreateBucket(bucketStr);
            var buckets = await _bucketService.GetBuckets();

            var bucket = buckets?.items.FirstOrDefault(x => x.bucketKey == bucketStr);
            var signedUrl = await _signedUrlService.GetSignedUrl(bucket.bucketKey, file.FileName);
            await _uploadFileService.Upload(signedUrl.urls[0], file.FileName, bytes);
            var uploadResponse =
                await _uploadFileService.FinishUpload(bucket.bucketKey, file.FileName, signedUrl.uploadKey);
            var base64Urn = Utils.Base64Encode(uploadResponse.objectId);
            var translateResponse = await _translationService.Translate(file.FileName, base64Urn, svf);

            var metadataLst = await _metadataService.GetMetadata(translateResponse.urn);
            var metadataMasterView = metadataLst.data.metadata.FirstOrDefault(x => x.isMasterView);

            var properties = await _metadataService.GetMetadataRVTMasterViewProperties(translateResponse.urn, metadataMasterView.guid);

            Console.WriteLine(properties.data.collection.Count());
            properties.data.collection.RemoveAll(x => x.properties.Dimensions?.Area == null);
            properties.data.collection.RemoveAll(x => x.properties.Dimensions?.Perimeter == null);
            properties.data.collection.RemoveAll(x => x.properties.Dimensions?.Volume == null);
            properties.data.collection.RemoveAll(x => x.properties.Dimensions == null);
            properties.data.collection.RemoveAll(x => x.properties.IdentityData?.Name == null);
            Console.WriteLine(properties.data.collection.Count());

            var rooms = new List<Rooms>();

            foreach (var item in properties.data.collection)
            {
                var room = new Rooms()
                {
                    Area = item.properties.Dimensions.Area,
                    Name = item.properties.IdentityData.Name,
                    Perimeter = item.properties.Dimensions.Perimeter,
                    Volume = item.properties.Dimensions.Volume
                };
                rooms.Add(room);
            }

            var details = new FullProperties()
            {
                Rooms = rooms
            };
            return Ok(details);
        }

        [HttpGet("getToken")]
        public async Task<IActionResult> getToken()
        {
            var token = await _bucketService.GetToken();
            return Ok(token);
        }

        [HttpPost("manualUpload")]
        public async Task<IActionResult> Post(IFormFile file, string bucketName, string svf)
        {
            var bytes = await file.GetBytes();
            var bucketStr = bucketName;

            await _bucketService.CreateBucket(bucketStr);
            var buckets = await _bucketService.GetBuckets();

            var bucket = buckets?.items.FirstOrDefault(x => x.bucketKey == bucketStr);
            var signedUrl = await _signedUrlService.GetSignedUrl(bucket.bucketKey, file.FileName);
            await _uploadFileService.Upload(signedUrl.urls[0], file.FileName, bytes);
            var uploadResponse =
                await _uploadFileService.FinishUpload(bucket.bucketKey, file.FileName, signedUrl.uploadKey);
            var base64Urn = Utils.Base64Encode(uploadResponse.objectId);
            var translateResponse = await _translationService.Translate(file.FileName, base64Urn, svf);

            return Ok(translateResponse);
        }

        [HttpPost("fullUpload")]
        public async Task<IActionResult> fullUpload(IFormFile file)
        {
            var bucketName = "bucket_poc";
            var svf = "svf2";
            var bytes = await file.GetBytes();
            var bucketStr = bucketName;

            await _bucketService.CreateBucket(bucketStr);
            var buckets = await _bucketService.GetBuckets();

            var bucket = buckets?.items.FirstOrDefault(x => x.bucketKey == bucketStr);
            var signedUrl = await _signedUrlService.GetSignedUrl(bucket.bucketKey, file.FileName);
            await _uploadFileService.Upload(signedUrl.urls[0], file.FileName, bytes);
            var uploadResponse =
                await _uploadFileService.FinishUpload(bucket.bucketKey, file.FileName, signedUrl.uploadKey);
            var base64Urn = Utils.Base64Encode(uploadResponse.objectId);
            var translateResponse = await _translationService.Translate(file.FileName, base64Urn, svf);

            var metadataList = await _metadataService.GetMetadata(translateResponse.urn);
            var metadata = metadataList.data.metadata.FirstOrDefault(x => x.role.ToLower() == "2d");
            var props = await _metadataService.GetFullProperties(translateResponse.urn, metadata?.guid);
            props.Urn = translateResponse.urn;
            return Ok(props);
        }

        [HttpPost("upload")]
        public async Task<IActionResult> Post([FromBody] FileUpload file, string svf)
        {
            var bucketStr = "bucket_poc";
            var buckets = await _bucketService.GetBuckets();

            var bucket = buckets?.items.FirstOrDefault(x => x.bucketKey == "bucket_poc");
            var signedUrl = await _signedUrlService.GetSignedUrl(bucket.bucketKey, file.FileName);
            await _uploadFileService.Upload(signedUrl.urls[0], file.FileName, file.Data);
            var uploadResponse =
                await _uploadFileService.FinishUpload(bucket.bucketKey, file.FileName, signedUrl.uploadKey);
            var base64Urn = Utils.Base64Encode(uploadResponse.objectId);
            var translateResponse = await _translationService.Translate(file.FileName, base64Urn, svf);

            return Ok(translateResponse);
        }

        [HttpGet("getTranslationProgress")]
        public async Task<IActionResult> GetTranslationProgress(string urn)
        {
            var translationStatus = await _translationService.GetTranslationStatus(urn);
            return Ok(translationStatus);
        }
    }
}