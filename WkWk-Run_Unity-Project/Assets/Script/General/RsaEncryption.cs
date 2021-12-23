using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.IO;
using System.Xml.Serialization;


public class RsaEncryption
{
    private static RSACryptoServiceProvider csp;
    public RSAParameters clientPrivateKey { get; private set; }
    public RSAParameters clientPublicKey { get; private set; }
    public RSAParameters serverPublicKey { get; set; }

    private static int MaxEncryptSize = 100;

    private string strServerPublicKey = $"<?xml version=\"1.0\" encoding=\"utf-16\"?>\n<RSAParameters xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">\n  <Exponent>AQAB</Exponent>\n  <Modulus>2xkVs60nk8a5sdGskTHN2ZVQjiAt4EJ7ZXlbDvz4oeezN1+SII6cVQSIT63U8+5kI8yRPFmpUYwhPAW+3aAV3T1noFvEfRfGuIacOxYu36cZb2+nK85meAGq7qeYKgYOqa0GyTut2RoROVylsSn6OxVHQTColaZMluALXRGZ8JVnUsH8Rq//XkePPvUfvKW2y3ek6VS37SIkEbfjSU/X3kcFu6woTMbEGcRRTsWUOEhhYtIK9DT2tTR4wxVM8OTHWVycixZbJ9F/3Ve9pMtJCvQ3IYH3EUw5VTZMpqqlUF4wHTNYx/hCS1pU4+fnjzP8iyCgrPy8Vh8CK2ETCAhUqQ==</Modulus>\n </RSAParameters>";

    public RsaEncryption()
    {
        csp = new RSACryptoServiceProvider(2048);
        clientPrivateKey = csp.ExportParameters(true);
        clientPublicKey = csp.ExportParameters(false);
        serverPublicKey = ConvertStringToKey(strServerPublicKey);
    }

    // ------------------------------------ General Method ---------------------------------------------------------
    public string Encrypt(string dataText, RSAParameters key)
    {
        if (key.Equals(null))
        {
            Console.WriteLine("Server public key not found");
            return null;
        }

        try
        {
            var rCsp = new RSACryptoServiceProvider();
            rCsp.ImportParameters(key);
            byte[] byteData = Encoding.Unicode.GetBytes(dataText);
            int readPos = 0;
            string encryptedData = string.Empty;

            while (byteData.Length - readPos > 0)
            {
                byte[] splitToEncrypt = new byte[MaxEncryptSize];

                if (byteData.Length - (readPos + MaxEncryptSize) > 0)
                {
                    Array.Copy(byteData, readPos, splitToEncrypt, 0, 100);
                    readPos += MaxEncryptSize;
                }
                else
                {
                    Array.Copy(byteData, readPos, splitToEncrypt, 0, byteData.Length - readPos);
                    readPos += byteData.Length - readPos;
                }

                byte[] encryptedByte = rCsp.Encrypt(splitToEncrypt, false);
                encryptedData += Convert.ToBase64String(encryptedByte);
                encryptedData += "|";
            }
                
            return encryptedData;
        }
        catch (Exception e)
        {
            Console.WriteLine("Error encrypt server : " + e.Message);
        }

        return null;
    }
    public string Decrypt(string dataCypher, RSAParameters key)
    {
        if (key.Equals(null))
        {
            Console.WriteLine("Empty key");
            return "Empty key";
        }

        try
        {
            var rCsp = new RSACryptoServiceProvider();
            rCsp.ImportParameters(key);
            string[] splitData = dataCypher.Split(new char[] {'|'} );
            string dataDecrypted = string.Empty;

            for(int i = 0; i < splitData.Length - 1; i++)
            {
                byte[] dataByte = Convert.FromBase64String(splitData[i]);
                byte[] dataPlain = rCsp.Decrypt(dataByte, false);
                dataDecrypted += Encoding.Unicode.GetString(dataPlain);
            }
                
            return dataDecrypted;
        }
        catch (Exception e)
        {
            Console.WriteLine("Decryption error : " + e.Message);
            return "Decryption error : " + e.Message;
        }

        return "Massage not encrypted";
    }

    public string ConvertKeyToString(RSAParameters key)
    {
        try
        {
            StringWriter sw = new StringWriter();
            XmlSerializer xs = new XmlSerializer(typeof(RSAParameters));
            xs.Serialize(sw, key);
            return sw.ToString();
        }
        catch (Exception e)
        {
            Console.WriteLine("Error Key To String : " + e.Message);
        }
        return null;
    }
    public RSAParameters ConvertStringToKey(String key)
    {
        try
        {
            StringReader reader = new StringReader(key);
            XmlSerializer xs = new XmlSerializer(typeof(RSAParameters));
            RSAParameters theKey = (RSAParameters)xs.Deserialize(reader);

            return theKey;
        }
        catch (Exception e)
        {
            Console.WriteLine("Error String To Key : " + e.Message);
        }

        RSAParameters emptyKey = new RSAParameters();
        return emptyKey;
    }
}

