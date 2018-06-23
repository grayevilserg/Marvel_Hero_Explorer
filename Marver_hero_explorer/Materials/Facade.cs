using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;

namespace Marver_hero_explorer.Materials
{
    public class Facade
    {
        private const string PrivateKey = "cd1cd6d81af8ccbd5bc275d410bda34bc747285e";
        private const string PublicKey = "eaf0958022fd8c5a5c079f58349ad7e7";
        private const int MaxChars = 500;
        private const string ImageNotAvailablePath = "http://i.annihil.us/u/prod/marvel/i/mg/b/40/image_not_available";

        private static string CalcDM5(string str)
        {
            var alg = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Md5);
            IBuffer buff = CryptographicBuffer.ConvertStringToBinary(str, BinaryStringEncoding.Utf8);
            var hashed = alg.HashData(buff);
            var res = CryptographicBuffer.EncodeToHexString(hashed);

            return res;
        }

        private static string CreateHash(string TimeStamp)
        {

            var ToHashed = TimeStamp + PrivateKey + PublicKey;
            var HashedMessage = CalcDM5(ToHashed);

            return HashedMessage;
        }

        private static async Task<CharacterDataWrapper> GetCharacterDataWrapperAsync()
        {
            Random Rand = new Random();
            var Offset = Rand.Next(1, MaxChars);

            string Url = string.Format("https://gateway.marvel.com/v1/public/characters?limit=10&offset={0}", Offset);

            var JsonMessage = await CallMarvelAsync(Url);

            //Deserialize

            var serializer = new DataContractJsonSerializer(typeof(CharacterDataWrapper));
            var MS = new MemoryStream(Encoding.UTF8.GetBytes(JsonMessage));

            var result = (CharacterDataWrapper)serializer.ReadObject(MS);
            return result;
        }

        private static async Task<ComicDataWrapper> GetComicDataWrapperAsync(int characterID)
        {
            string Url = string.Format("https://gateway.marvel.com:443/v1/public/comics?characters={0}&limit=10",
                   characterID);

            var JsonMessage = await CallMarvelAsync(Url);

            //Deserialize

            var serializer = new DataContractJsonSerializer(typeof(ComicDataWrapper));
            var MS = new MemoryStream(Encoding.UTF8.GetBytes(JsonMessage));

            var result = (ComicDataWrapper)serializer.ReadObject(MS);
            return result;
        }

        public static async Task PopulateMarvelCharactersAsync(ObservableCollection<Character> MarvelCharacters)
        {
            try
            {
                var characterDataWrapper = await GetCharacterDataWrapperAsync();
                var characters = characterDataWrapper.data.results;

                foreach (var character in characters)
                {
                    //Filter characters that are missing thumbnail images
                    if (character.thumbnail != null && character.thumbnail.path != "" && character.thumbnail.path != ImageNotAvailablePath)
                    {
                        character.thumbnail.small = string.Format("{0}/standard_small.{1}", character.thumbnail.path, character.thumbnail.extension);
                        character.thumbnail.large = string.Format("{0}/portrait_xlarge.{1}", character.thumbnail.path, character.thumbnail.extension);
                        MarvelCharacters.Add(character);
                    }
                }
            }
            catch (Exception)
            {
                return;
            }
        }

        public static async Task PopulateMarvelComicsAsync(int characterID, ObservableCollection<ComicBook> MarvelComics)
        {
            try
            {
                var ComicDataWrapper = await GetComicDataWrapperAsync(characterID);
                var Comics = ComicDataWrapper.data.results;

                foreach (var comic in Comics)
                {
                    //Filter characters that are missing thumbnail images
                    if (comic.thumbnail != null && comic.thumbnail.path != "" && comic.thumbnail.path != ImageNotAvailablePath)
                    {
                        comic.thumbnail.small = string.Format("{0}/portrait_medium.{1}", comic.thumbnail.path, comic.thumbnail.extension);
                        comic.thumbnail.large = string.Format("{0}/portrait_xlarge.{1}", comic.thumbnail.path, comic.thumbnail.extension);
                        MarvelComics.Add(comic);
                    }
                }
            }
            catch (Exception)
            {
                return;
            }
        }

        private static async Task<string> CallMarvelAsync(string url)
        {
            //Get MD5 hash
            var TimeStamp = DateTime.Now.Ticks.ToString();
            var Hash = CreateHash(TimeStamp);

            string CompleteUrl = string.Format("{0}&apikey={1}&ts={2}&hash={3}", url, PublicKey, TimeStamp, Hash);

            //Call to Marvel side, creation josn object

            HttpClient Http = new HttpClient();
            var Response = await Http.GetAsync(CompleteUrl);
            return await Response.Content.ReadAsStringAsync();
        }
    }
}
