using UnityEngine;
using System.Text;
using System.IO;

public class FileAES
{
    protected AES.AesCryptography aes = new AES.AesCryptography();
    private FileAES() { }
    public FileAES(string str) { aes = new AES.AesCryptography(str); }
}

public class AESLoader:FileAES
{
    private string str = null;

    private AESLoader() : base(null) { }
    public AESLoader(string Key) : base(Key) { }

    public int Load(string path, Encoding encode)
    {
        try
        {
            FileStream fs = new FileStream(Application.persistentDataPath + "/" + path, FileMode.Open, FileAccess.Read);
            BinaryReader br = new BinaryReader(fs);
            var bin = new System.Collections.Generic.List<byte>();
            try
            {
                while (true) { bin.Add(br.ReadByte()); }
            }
            catch (EndOfStreamException) { }
            str = encode.GetString(aes.Decrypt(bin.ToArray()));
            br.Close();
        }
        catch {
            // ロード失敗
            return -1;
        }
        return 0;
    }

    public string GetString()
    {
        return str;
    }
}

public class AESWriter:FileAES
{
    private AESWriter() : base(null) { }
    public AESWriter(string Key) : base(Key) { }

    public int Save(string path, string dat,Encoding encode)
    {
        try
        {
            FileStream fs = new FileStream(Application.persistentDataPath + "/" + path, FileMode.Create, FileAccess.Write);
            BinaryWriter sw = new BinaryWriter(fs);
            var bin = aes.Encrypt(encode.GetBytes(dat));
            sw.Write(bin);
            sw.Flush();
            sw.Close();
        }
        catch {
            // セーブ失敗
            return -1;
        }
        return 0;
    }
}

