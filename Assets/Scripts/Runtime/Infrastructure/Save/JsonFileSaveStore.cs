using System;
using System.IO;
using CurioClerk.Core.Progression;
using UnityEngine;

namespace CurioClerk.Infrastructure.Save
{
    public sealed class JsonFileSaveStore : ISaveStore
    {
        private readonly string _path;
        private readonly string _backupPath;
        private readonly string _temporaryPath;

        public JsonFileSaveStore(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException("Save path is required.", nameof(path));
            }

            _path = path;
            _backupPath = path + ".bak";
            _temporaryPath = path + ".tmp";
        }

        public PlayerSaveData LoadOrDefault()
        {
            if (TryRead(_path, out var primary))
            {
                return primary;
            }

            if (TryRead(_backupPath, out var backup))
            {
                return backup;
            }

            return new PlayerSaveData();
        }

        public void Save(PlayerSaveData data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            data.Sanitize();
            var directory = Path.GetDirectoryName(_path);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(_temporaryPath, JsonUtility.ToJson(data, true));
            if (File.Exists(_path))
            {
                File.Replace(_temporaryPath, _path, _backupPath, true);
            }
            else
            {
                File.Move(_temporaryPath, _path);
            }
        }

        private static bool TryRead(string path, out PlayerSaveData data)
        {
            data = null;
            if (!File.Exists(path))
            {
                return false;
            }

            try
            {
                data = JsonUtility.FromJson<PlayerSaveData>(File.ReadAllText(path));
                if (data == null || data.version <= 0 || data.version > PlayerSaveData.CurrentVersion)
                {
                    data = null;
                    return false;
                }

                data.Sanitize();
                return true;
            }
            catch (Exception exception) when (exception is ArgumentException || exception is IOException)
            {
                data = null;
                return false;
            }
        }
    }
}
