using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MemeTV.Models;
using Newtonsoft.Json;

namespace MemeTV.BusinessLogic
{
    public class ClipManager : IClipManager
    {
        private readonly ISqlConnectionProvider sqlConnectionProvider;
        private readonly IClipProvider clipProvider;
        private readonly IClipIdentifierProvider idProvider;
        private readonly IVttTemplateRenderer vttRenderer;

        public ClipManager(
            ISqlConnectionProvider sqlConnectionProvider,
            IClipProvider clipProvider,
            IClipIdentifierProvider idProvider,
            IVttTemplateRenderer vttRenderer)
        {
            this.sqlConnectionProvider = sqlConnectionProvider;
            this.clipProvider = clipProvider;
            this.idProvider = idProvider;
            this.vttRenderer = vttRenderer;
        }

        public async Task<Subtitles> GetAsync(string id)
        {
            using (var db = sqlConnectionProvider.Get())
            {
                await db.OpenAsync();

                try
                {
                    var cmd = db.CreateCommand();
                    cmd.CommandText = "SELECT Id, Name, Email, ClipName, Captions, Created FROM [Subtitles] WHERE Id = @Id";
                    AddParameter(cmd, "Id", id);
                    var reader = await cmd.ExecuteReaderAsync();
                    if (!await reader.ReadAsync())
                    {
                        return null;
                    }
                    var result_id = reader.GetString(reader.GetOrdinal("Id"));
                    var name = reader.GetString(reader.GetOrdinal("Name"));
                    var email = reader.GetString(reader.GetOrdinal("Email"));
                    var clipName = reader.GetString(reader.GetOrdinal("ClipName"));
                    var captions = JsonConvert.DeserializeObject<List<string>>(reader.GetString(reader.GetOrdinal("Captions")));
                    var created = reader.GetDateTime(reader.GetOrdinal("Created"));
                    return new Subtitles
                    {
                        Id = result_id,
                        ClipName = clipName,
                        Created = created,
                        Email = email,
                        Name = name,
                        Captions = captions
                    };
                }
                catch (System.Exception exc)
                {
                    throw exc;
                }
                finally
                {
                    db.Close();
                }
            }
        }

        public async Task<string> StoreAsync(string name, string email, string clip, string[] modelSubtitles)
        {
            var sub = new Subtitles
            {
                Id = idProvider.Get(),
                Email = email,
                Name = name,
                Created = DateTime.UtcNow,
                ClipName = clip,
                Captions = modelSubtitles.ToList()
            };

            using (var db = sqlConnectionProvider.Get())
            {
                await db.OpenAsync();

                var cmd = db.CreateCommand();
                cmd.CommandText = "insert into [Subtitles] (Id, Name, Email, ClipName, Captions, Created) VALUES (@Id, @Name, @Email, @ClipName, @Captions, @Created)";

                AddParameter(cmd, "Id", sub.Id);
                AddParameter(cmd, "Name", sub.Name);
                AddParameter(cmd, "Email", sub.Email);
                AddParameter(cmd, "ClipName", sub.ClipName);
                AddParameter(cmd, "Captions", JsonConvert.SerializeObject(sub.Captions));
                AddParameter(cmd, "Created", sub.Created);

                var changeCount = await cmd.ExecuteNonQueryAsync();
                if (changeCount == 0)
                {
                    // uh oh.
                }

                db.Close();
            }

            return sub.Id;
        }

        public async Task<UserClip> GetClipSubtitleAsync(string id)
        {
            var subtitles = await GetAsync(id);
            var clip = clipProvider.Get(subtitles.ClipName);
            return new UserClip
            {
                //Email = subtitles.Email,
                Name = subtitles.Name,
                Created = subtitles.Created,
                VTT = "/api/subtitles/vtt/" + id,
                ClipName = clip.Name
            };
        }

        public async Task<string> GetClipVttAsync(string id)
        {
            var subtitles = await GetAsync(id);
            var clip = clipProvider.Get(subtitles.ClipName);
            return vttRenderer.Render(clip, subtitles.Captions);
        }

        public async Task<string> GetEmptyVttAsync(string clipName)
        {
            if (int.TryParse(clipName, out _)) clipName = "clip" + clipName;
            var clip = clipProvider.Get(clipName);
            return vttRenderer.Render(clip, Enumerable.Range(0, clip.CaptionCues.Count).Select(x => ""));
        }

        public async Task<ClipHeader[]> GetHeadersAsync()
        {
            return clipProvider.GetHeaders();
        }
        private static void AddParameter(DbCommand cmd, string key, object value)
        {
            var nameParam = cmd.CreateParameter();
            nameParam.ParameterName = key;
            nameParam.DbType = value is DateTime
                ? System.Data.DbType.DateTime
                : System.Data.DbType.String;
            nameParam.Value = value;
            cmd.Parameters.Add(nameParam);
        }
    }
}