using System.IO;
using System.Security.Cryptography;

namespace AES {
    //----------------------------------
    // AES暗号化コード
    public class AesCryptography {

        // 256bit(32byte)のInitVector（初期ベクタ）とKey（暗号キー）
        private const string AesInitVector = @"H401_AESEnctyptSystemFirstVector";
        private string AesKey;

        private const int BlockSize = 256;
        private const int KeySize = 256;

        public AesCryptography(string Key = null) {
            if(Key != null) {
                AesKey = Key;
            }
        }

        /// <summary>
        /// 暗号化スクリプト
        /// </summary>
        /// <returns>byte[] 暗号化したbyte列</returns>
        public byte[] Encrypt(byte[] binData) {
            RijndaelManaged myRijndael = new RijndaelManaged();
            myRijndael.Padding = PaddingMode.Zeros;
            myRijndael.Mode = CipherMode.CBC;
            myRijndael.KeySize = KeySize;
            myRijndael.BlockSize = BlockSize;

            byte[] key = new byte[0];
            byte[] InitVector = new byte[0];

            key = System.Text.Encoding.UTF8.GetBytes(AesKey);
            InitVector = System.Text.Encoding.UTF8.GetBytes(AesInitVector);

            ICryptoTransform encryptor = myRijndael.CreateEncryptor(key, InitVector);

            MemoryStream msEncrypt = new MemoryStream();
            CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);

            byte[] src = binData;

            // 暗号化する
            try
            {
                csEncrypt.Write(src, 0, src.Length);
                csEncrypt.FlushFinalBlock();
            }
            catch { return null; }

            byte[] dest = msEncrypt.ToArray();

            return dest;
        }



        /// <summary>
        /// 複合化スクリプト
        /// </summary>
        /// <returns>byte[] 複合化したbyte列</returns>
        public byte[] Decrypt(byte[] binData) {

            RijndaelManaged myRijndael = new RijndaelManaged();
            myRijndael.Padding = PaddingMode.Zeros;
            myRijndael.Mode = CipherMode.CBC;
            myRijndael.KeySize = KeySize;
            myRijndael.BlockSize = BlockSize;

            byte[] key = new byte[0];
            byte[] InitVector = new byte[0];

            key = System.Text.Encoding.UTF8.GetBytes(AesKey);
            InitVector = System.Text.Encoding.UTF8.GetBytes(AesInitVector);

            ICryptoTransform decryptor = myRijndael.CreateDecryptor(key, InitVector);
            byte[] src = binData;
            byte[] dest = new byte[src.Length];

            MemoryStream msDecrypt = new MemoryStream(src);
            CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);

            // 複号化する
            try
            {
                csDecrypt.Read(dest, 0, dest.Length);
            }
            catch { return null; }

            return dest;
        }
    }
}
