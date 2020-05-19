using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureKeyObfuscation
{
   public static class AzureSecretsObfuscation
   {
       /// <summary>
       /// Avoid using magic strings in code
       /// </summary>
        #region Constants
        private const string OutfileName = "AevaCodeExercise.output.json";
       private const string ContainerRegistryPassword = "ContainerRegistry-Password";
       private const string CosmosDbKey = "CosmosDB-Key";
       private const string ClientSecret = "ClientSecret";
       private const string StorageAccount = "StorageAccount";
       private const string AccountKey = "AccountKey";
       private const string ConnectionString = "ConnectionString";
       private const string Password = "password";

        #endregion  

        /// <summary>
        /// Obfuscates the Azure Secrets that need to be masked
        /// </summary>
        /// <param name="inputFile"></param>
        /// <returns></returns>
        public static async Task ObfuscateAzureSecrets(this string inputFile)
        {
           try
           {
               var infilePath = await GetFilePath(inputFile);
               var outfilePath = await GetFilePath(OutfileName);
               await File.WriteAllTextAsync(outfilePath, string.Empty);
               await using var readStream= File.OpenRead(infilePath);
               using var reader = new StreamReader(readStream);
               await using var writeStream = File.AppendText(outfilePath);
             
               string configLine;
               while ((configLine = await reader.ReadLineAsync()) != null)
               {
                   await WriteToOutputFile(configLine, writeStream);
               }
           }
           catch (Exception e)
           {
               Console.WriteLine(e);
               throw;
           }
        }

        /// <summary>
        /// Writes the obfuscated values along with other config values to file
        /// </summary>
        /// <param name="configLine"></param>
        /// <param name="writeStream"></param>
        /// <returns></returns>
        private static async Task WriteToOutputFile(string configLine, StreamWriter writeStream)
        {
            if (configLine.Contains(ContainerRegistryPassword, StringComparison.CurrentCultureIgnoreCase)
                || configLine.Contains(CosmosDbKey, StringComparison.CurrentCultureIgnoreCase)
                || configLine.Contains(ClientSecret, StringComparison.CurrentCultureIgnoreCase))
            {
                var configArray = configLine.Split(": ");
                await writeStream.WriteLineAsync(await configLine.MaskLineSecret(configArray[1]));
            }

            else if (configLine.Contains(StorageAccount, StringComparison.CurrentCultureIgnoreCase) &&
                     configLine.Contains(AccountKey, StringComparison.CurrentCultureIgnoreCase))

            {
                var secret = await configLine.ExtractLineSecret(AccountKey);
                await writeStream.WriteLineAsync(await configLine.MaskLineSecret(secret));
            }
            else if (configLine.Contains(ConnectionString, StringComparison.CurrentCultureIgnoreCase) &&
                     configLine.Contains(Password, StringComparison.CurrentCultureIgnoreCase))
            {
                var secret = await configLine.ExtractLineSecret(Password);
                await writeStream.WriteLineAsync(await configLine.MaskLineSecret(secret));
            }
            else
                await writeStream.WriteLineAsync(configLine);
        }

        /// <summary>
        /// Masks the secret
        /// </summary>
        /// <param name="line"></param>
        /// <param name="secret"></param>
        /// <returns></returns>
        private static async Task<string> MaskLineSecret(this string line,string secret)
        {
            await Task.Delay(0);
           if (secret.Length <= 10) 
                return secret;
           var initialCharacters = secret.Substring(0, 5);
           var finalCharacters = secret.Substring(secret.Length - 5, 5);
           var maskedCharacters = new string("...");
           var newSection= $"{initialCharacters}{maskedCharacters}{finalCharacters}";
           return line.Replace(secret, newSection);
         }

        /// <summary>
        /// Extracts the secret that needs to be masked
        /// </summary>
        /// <param name="line"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        private static async Task<string> ExtractLineSecret(this string line, string key)
        {
            await Task.Delay(0);
            switch (key)
            {
                case AccountKey:
                    var strKey = line.Split(";");
                    return await GetSecret(strKey,AccountKey);
                case Password:
                    var strPwd = line.Split(",");
                    return await GetSecret(strPwd,Password);
            }
            return line;
        }

        /// <summary>
        /// Extract the secret from 
        /// </summary>
        /// <param name="strArray"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        private static async Task<string> GetSecret(string[] strArray, string key)
        {
            await Task.Delay(0);
            var section = strArray.SingleOrDefault(s => s.StartsWith(key, 
                                                    StringComparison.CurrentCultureIgnoreCase));
            var index = section?.IndexOf("=");
            var secret = section?.Substring(index.GetValueOrDefault() + 1);
            return secret;
        }


        /// <summary>
        /// Gets the path of the input and output files
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        private static async Task<string> GetFilePath(string file)
        {
            await Task.Delay(0);
            var parent = Directory.GetParent(Directory.GetCurrentDirectory()).Parent;
            if (parent?.Parent?.Parent == null) return string.Empty;
            var filePath = $"{parent.Parent?.Parent.FullName}\\{file}";
            return filePath;

        }

    }
}
