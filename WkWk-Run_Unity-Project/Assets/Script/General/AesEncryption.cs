using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Security.Cryptography;
using System.Text;


public class AesEncryption
{
    public AesCryptoServiceProvider aesKey { get; private set; }
    byte[] keyIV = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

    public AesEncryption()
    {
        aesKey = new AesCryptoServiceProvider();
        aesKey.IV = keyIV;
    }

    public string Encrypt(string data)
    {
        try
        {
            ICryptoTransform transform = aesKey.CreateEncryptor();
            byte[] dataByte = Encoding.ASCII.GetBytes(data);
            byte[] encryptedByte = transform.TransformFinalBlock(dataByte, 0, dataByte.Length);
            string encryptedData = Convert.ToBase64String(encryptedByte);
            return encryptedData;
        }
        catch (Exception e)
        {
            Console.WriteLine("Error Aes encrypt : " + e.Message);
        }

        return null;
    }
    public string Decrypt(string data)
    {
        try
        {
            ICryptoTransform transform = aesKey.CreateDecryptor();
            byte[] encryptedByte = Convert.FromBase64String(data);
            byte[] decryptedByte = transform.TransformFinalBlock(encryptedByte, 0, encryptedByte.Length);
            string decryptedData = Encoding.ASCII.GetString(decryptedByte);
            return decryptedData;
        }
        catch(Exception e)
        {
            return "Error Aes decrypt : " + e.Message;
        }

        return null;
    }

    public string ConvertKeyToString(AesCryptoServiceProvider key)
    {
        string theKey = Convert.ToBase64String(key.Key);
        return theKey;
    }
    public AesCryptoServiceProvider ConvertStringToKey(string key)
    {
        AesCryptoServiceProvider aesCrypto = new AesCryptoServiceProvider();
        aesCrypto.Key = Convert.FromBase64String(Split64(key));
        aesCrypto.IV = keyIV;
        return aesCrypto;
    }

    public void SetKey(AesCryptoServiceProvider aesCrypto)
    {
        aesKey = aesCrypto;
    }

    private string Split64(string data)
    {
        string[] spt = data.Split('=');
        return spt[0] + "=";
    }
}

