using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
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

        public async Task ReportBadCaptionsAsync(string clip)
        {
            var targetClip = this.clipProvider.Get(clip);
            if (targetClip == null) return;

            using (var db = sqlConnectionProvider.Get())
            {
                await db.OpenAsync();
                var cmd = db.CreateCommand();
                cmd.CommandText = "INSERT INTO [BadCaptions] (Id, ClipName, Reported) VALUES (@Id, @ClipName, @Reported)";
                AddParameter(cmd, "Id", Guid.NewGuid());
                AddParameter(cmd, "ClipName", clip);
                AddParameter(cmd, "Reported", DateTime.UtcNow);
                await cmd.ExecuteNonQueryAsync();
                db.Close();
            }
        }

        public async Task<long> UpdateLikesAsync(string id, bool liked)
        {
            var data = await GetAndIncrementSocialData(id, 0, liked ? 1 : -1);
            return data.Likes;
        }

        public async Task<Subtitles> GetAsync(string id)
        {
            using (var db = sqlConnectionProvider.Get())
            {
                await db.OpenAsync();

                try
                {
                    var cmd = db.CreateCommand();
                    cmd.CommandText = "SELECT Id, Name, Email, Title, Description, ClipName, Captions, Created FROM [Subtitles] WHERE Id = @Id";
                    AddParameter(cmd, "Id", id);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (!await reader.ReadAsync())
                        {
                            return null;
                        }
                        var result_id = reader.GetString(reader.GetOrdinal("Id"));
                        var name = reader.GetString(reader.GetOrdinal("Name"));
                        var email = reader.GetString(reader.GetOrdinal("Email"));
                        var clipName = reader.GetString(reader.GetOrdinal("ClipName"));

                        var title = reader.GetValue(reader.GetOrdinal("Title"));
                        var description = reader.GetValue(reader.GetOrdinal("Description"));

                        var captions = JsonConvert.DeserializeObject<List<string>>(reader.GetString(reader.GetOrdinal("Captions")));
                        var created = reader.GetDateTime(reader.GetOrdinal("Created"));
                        return new Subtitles
                        {
                            Id = result_id,
                            ClipName = clipName,
                            Created = created,
                            Email = email,
                            Name = name,
                            Title = title?.ToString(),
                            Description = description?.ToString(),
                            Captions = captions
                        };
                    }
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

        public async Task<string> StoreAsync(string name, string email, string title, string description, string clip, string[] modelSubtitles)
        {
            var sub = new Subtitles
            {
                Id = idProvider.Get(),
                Email = email,
                Name = name,
                Title = title,
                Description = description,
                Created = DateTime.UtcNow,
                ClipName = clip,
                Captions = modelSubtitles.ToList()
            };

            using (var db = sqlConnectionProvider.Get())
            {
                await db.OpenAsync();

                var cmd = db.CreateCommand();
                cmd.CommandText = "insert into [Subtitles] (Id, Name, Email, Title, Description, ClipName, Captions, Created) VALUES (@Id, @Name, @Email, @Title, @Description, @ClipName, @Captions, @Created)";

                AddParameter(cmd, "Id", sub.Id);
                AddParameter(cmd, "Name", sub.Name);
                AddParameter(cmd, "Email", sub.Email);
                AddParameter(cmd, "ClipName", sub.ClipName);
                AddParameter(cmd, "Title", sub.Title);
                AddParameter(cmd, "Description", sub.Description);
                AddParameter(cmd, "Captions", JsonConvert.SerializeObject(sub.Captions));
                AddParameter(cmd, "Created", sub.Created);

                var changeCount = await cmd.ExecuteNonQueryAsync();
                if (changeCount > 0)
                {
                    await CreateSocialDataAsync(sub.Id, db);
                }

                db.Close();
            }

            return sub.Id;
        }

        public async Task<UserClip> GetClipSubtitleAsync(string id, bool updateViewCount)
        {
            var subtitles = await GetAsync(id);
            var socialData = updateViewCount ? await GetAndIncrementSocialData(id, 1, 0) : await GetSocialDataAsync(id);
            var clip = clipProvider.Get(subtitles.ClipName);
            return new UserClip
            {
                //Email = subtitles.Email,
                Name = subtitles.Name,
                Created = subtitles.Created,
                Title = subtitles.Title,
                Description = subtitles.Description,
                Likes = socialData.Likes,
                Views = socialData.Views,
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


        private async Task CreateSocialDataAsync(string id, DbConnection db = null)
        {
            var dispose = false;
            if (db == null)
            {
                dispose = true;
                db = sqlConnectionProvider.Get();
                await db.OpenAsync();
            }
            var cmd = db.CreateCommand();
            cmd.CommandText = "INSERT INTO [SubtitlesSocialData] (Id, SubtitlesId, Views, Likes) VALUES (@Id, @SubtitlesId, 0, 0)";
            AddParameter(cmd, "Id", Guid.NewGuid());
            AddParameter(cmd, "SubtitlesId", id);
            await cmd.ExecuteNonQueryAsync();
            if (dispose) db.Close();
        }


        private async Task UpdateSocialDataAsync(string subtitlesId, long views, long likes, DbConnection db = null)
        {
            var dispose = false;
            if (db == null)
            {
                dispose = true;
                db = sqlConnectionProvider.Get();
                await db.OpenAsync();
            }
            var cmd = db.CreateCommand();
            cmd.CommandText = "UPDATE [SubtitlesSocialData] SET Views = @Views, Likes = @Likes WHERE SubtitlesId = @Id";
            AddParameter(cmd, "Id", subtitlesId);
            AddParameter(cmd, "Views", views);
            AddParameter(cmd, "Likes", likes);
            await cmd.ExecuteNonQueryAsync();
            if (dispose) db.Close();
        }

        private async Task<SubtitlesSocialData> GetAndIncrementSocialData(string id, int viewIncrement, int likeIncrement)
        {
            using (var db = sqlConnectionProvider.Get())
            {
                await db.OpenAsync();
                var data = await GetSocialDataAsync(id, db);
                await UpdateSocialDataAsync(data.SubtitlesId, data.Views + viewIncrement, data.Likes + likeIncrement);
                data.Views += viewIncrement;
                data.Likes += likeIncrement;
                db.Close();
                return data;
            }
        }

        private async Task<SubtitlesSocialData> GetSocialDataAsync(string id, DbConnection db = null)
        {
            var dispose = false;
            if (db == null)
            {
                dispose = true;
                db = sqlConnectionProvider.Get();
                await db.OpenAsync();
            }

            try
            {
                var cmd = db.CreateCommand();
                cmd.CommandText = "SELECT * FROM [SubtitlesSocialData] WHERE SubtitlesId = @Id";
                AddParameter(cmd, "Id", id);
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (!await reader.ReadAsync())
                    {
                        reader.Close();
                        await CreateSocialDataAsync(id, db);
                        return new SubtitlesSocialData { SubtitlesId = id };
                    }
                    var result_id = reader.GetGuid(reader.GetOrdinal("Id"));
                    var views = reader.GetInt64(reader.GetOrdinal("Views"));
                    var likes = reader.GetInt64(reader.GetOrdinal("Likes"));
                    var subtitlesId = reader.GetString(reader.GetOrdinal("SubtitlesId"));
                    return new SubtitlesSocialData
                    {
                        Id = result_id,
                        Views = views,
                        Likes = likes,
                        SubtitlesId = subtitlesId
                    };
                }
            }
            finally
            {
                if (dispose) db.Close();
            }
        }

        private static void AddParameter(DbCommand cmd, string key, object value)
        {
            var nameParam = cmd.CreateParameter();
            nameParam.ParameterName = key;
            nameParam.DbType =
                value is long ? System.Data.DbType.Int64 :
                value is Guid ? System.Data.DbType.Guid :
                value is DateTime ? System.Data.DbType.DateTime : System.Data.DbType.String;
            nameParam.Value = value;
            cmd.Parameters.Add(nameParam);
        }
    }
}