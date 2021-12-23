using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class EditTestTry
    {
        [Test]
        public void SaveDataTest()
        {
            var saveData = new SaveData();
            // Check skin price
            Assert.AreEqual(0, saveData.SkinPrice[0]);
            Assert.AreEqual(1000, saveData.SkinPrice[1]);
            Assert.AreEqual(2000, saveData.SkinPrice[2]);
            Assert.AreEqual(2500, saveData.SkinPrice[3]);
            Assert.AreEqual(3000, saveData.SkinPrice[4]);
            // Check default coin
            Assert.AreEqual(500, saveData.Coin);
            // Check characters name
            Assert.AreEqual("Bocil", saveData.characterName[0]);
            Assert.AreEqual("Cewe Kepang", saveData.characterName[1]);
            Assert.AreEqual("Emak Kos", saveData.characterName[2]);
            Assert.AreEqual("Pak Ustadz", saveData.characterName[3]);
            Assert.AreEqual("Pocong", saveData.characterName[4]);
        }

       [Test]
       public void EncryptionTest()
       {
            // String Data
            string testData = "Test|DataString";

            // RSA or Asymmetric encryption
            // Server public key
            var rsa = new RsaEncryption();
            Assert.AreNotEqual(testData, rsa.Encrypt(testData, rsa.serverPublicKey));
            // Client public and private key
            string encryptedData = rsa.Encrypt(testData, rsa.clientPublicKey);
            int lenght = testData.Length;
            string decryptedData = rsa.Decrypt(encryptedData, rsa.clientPrivateKey).Substring(0, lenght);
            Assert.AreEqual(testData, decryptedData);

            // AES or Symmetric key
            var aes = new AesEncryption();
            string encryptedAESData = aes.Encrypt(testData);
            Assert.AreEqual(testData, aes.Decrypt(encryptedAESData));
       }
    }
}
