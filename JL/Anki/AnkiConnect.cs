﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using JL.Utilities;

namespace JL.Anki
{
    public static class AnkiConnect
    {
        public static async Task<Response> AddNoteToDeck(Note note)
        {
            Request req = new("addNote", 6, new Dictionary<string, object> { { "note", note } });
            return await Send(req).ConfigureAwait(false);
        }

        public static async Task<Response> GetDeckNames()
        {
            Request req = new("deckNames", 6);
            return await Send(req).ConfigureAwait(false);
        }

        public static async Task<Response> GetModelNames()
        {
            Request req = new("modelNames", 6);
            return await Send(req).ConfigureAwait(false);
        }

        public static async Task<Response> GetModelFieldNames(string modelName)
        {
            Request req = new("modelFieldNames", 6, new Dictionary<string, object> { { "modelName", modelName } });
            return await Send(req).ConfigureAwait(false);
        }

        public static async Task<Response> StoreMediaFile(string filename, string data)
        {
            Request req = new("storeMediaFile", 6,
                new Dictionary<string, object> { { "filename", filename }, { "data", data } });
            return await Send(req).ConfigureAwait(false);
        }

        public static async Task<Response> Sync()
        {
            Request req = new("sync", 6);
            return await Send(req).ConfigureAwait(false);
        }

        public static async Task<byte[]> GetAudioFromJpod101(string foundSpelling, string reading)
        {
            try
            {
                Uri uri = new(
                    "http://assets.languagepod101.com/dictionary/japanese/audiomp3.php?kanji=" +
                    foundSpelling +
                    "&kana=" +
                    reading
                );
                HttpResponseMessage getResponse = await Storage.Client.GetAsync(uri).ConfigureAwait(false);
                // todo mining storemediafile thingy
                return await getResponse.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Utils.Alert(AlertLevel.Error, "Error getting audio from jpod101");
                Utils.Logger.Error(e, "Error getting audio from jpod101");
                return null;
            }
        }

        public static async Task<bool> CheckAudioField(long noteId, string audioFieldName)
        {
            Request req = new("notesInfo", 6,
                new Dictionary<string, object> { { "notes", new List<long> { noteId } } });

            Response response = await Send(req).ConfigureAwait(false);
            if (response != null)
            {
                Dictionary<string, Dictionary<string, object>> fields =
                    JsonSerializer.Deserialize<List<NotesInfoResult>>(
                        response.Result.ToString()!)![0].Fields;

                return fields[audioFieldName]["value"].ToString() != "";
            }
            else
            {
                Utils.Alert(AlertLevel.Error, "Error checking audio field");
                Utils.Logger.Error("Error checking audio field");
                return false;
            }
        }

        private static async Task<Response> Send(Request req)
        {
            try
            {
                // AnkiConnect doesn't like null values
                StringContent payload = new(JsonSerializer.Serialize(req,
                    new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull }));
                Utils.Logger.Information("Sending: " + await payload.ReadAsStringAsync().ConfigureAwait(false));

                HttpResponseMessage postResponse = await Storage.Client.PostAsync(ConfigManager.AnkiConnectUri, payload)
                    .ConfigureAwait(false);

                Response json = await postResponse.Content.ReadFromJsonAsync<Response>().ConfigureAwait(false);
                Utils.Logger.Information("json result: " + json!.Result);

                if (json!.Error == null) return json;

                Utils.Alert(AlertLevel.Error, json.Error.ToString());
                Utils.Logger.Error(json.Error.ToString());
                return null;
            }
            catch (HttpRequestException e)
            {
                Utils.Alert(AlertLevel.Error, "Communication error: Is Anki open?");
                Utils.Logger.Error(e, "Communication error: Is Anki open?");
                return null;
            }
            catch (Exception e)
            {
                Utils.Alert(AlertLevel.Error, "Communication error: Unknown error");
                Utils.Logger.Error(e, "Communication error: Unknown error");
                return null;
            }
        }
    }
}
