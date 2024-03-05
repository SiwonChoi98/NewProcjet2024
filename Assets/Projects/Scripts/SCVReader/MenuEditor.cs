using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using PlayFab.ProfilesModels;
using Unity.VisualScripting;
using UnityEngine.UI;
using Newtonsoft.Json;
using System.Security.Cryptography;

public class MenuEditor
{
    public static string csvPath = ".csv";
    public static string csvFilePath = "Assets/Resources/Spec/";
    public static string resourcesLoadAllPath = "Spec/";
    public static string encryptedcsvFilePath = "Assets/Resources/Spec/";

    private static string encryptionKey = "aksn&%#!wjsaghws#$$%%@lwjka";
    
    
    [MenuItem("Custom/CSV 파일 암호화/적용하기")]
    public static void EncryptCSV()
    {
        TextAsset[] textAsset = Resources.LoadAll<TextAsset>(resourcesLoadAllPath);
        
        //암호화된 파일 생성
        for (int i = 0; i < textAsset.Length; i++)
        {
            csvFilePath = "Assets/Resources/Spec/";
            encryptedcsvFilePath = "Assets/Resources/Spec/";
            
            string fileName = textAsset[i].name;
            
            csvFilePath += fileName;
            csvFilePath += csvPath;

            encryptedcsvFilePath += fileName;
            encryptedcsvFilePath += "Encrypt";
            encryptedcsvFilePath += csvPath;
            
            byte[] keyBytes = new byte[32];
            RandomNumberGenerator.Create().GetBytes(keyBytes);
            string encryptionKey = Convert.ToBase64String(keyBytes);
        
            byte[] key = Convert.FromBase64String(encryptionKey);
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = key;
                aesAlg.GenerateIV();

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                using (FileStream fsOutput = new FileStream(encryptedcsvFilePath, FileMode.Create))
                {
                    fsOutput.Write(aesAlg.IV, 0, aesAlg.IV.Length);
                    using (CryptoStream csEncrypt = new CryptoStream(fsOutput, encryptor, CryptoStreamMode.Write))
                    {
                        using (FileStream fsInput = new FileStream(csvFilePath, FileMode.Open))
                        {
                            fsInput.CopyTo(csEncrypt);
                        }
                    }
                }
            }
            
            Debug.Log(fileName + "암호화 성공");
        }

        //기존 파일 삭제
        for (int i = 0; i < textAsset.Length; i++)
        {
            string fileName = textAsset[i].name;
            
            csvFilePath = "Assets/Resources/Spec/";
            
            csvFilePath += fileName;
            csvFilePath += csvPath;
            
            File.Delete(csvFilePath);
            
            Debug.Log(fileName + " 기존 파일 삭제");
        }
        
    }

    //[MenuItem("Custom/CSV 파일 복호화/적용하기")]
    public static void DecryptCSV()
    {
        byte[] keyBytes = new byte[32];
        RandomNumberGenerator.Create().GetBytes(keyBytes);
        encryptionKey = Convert.ToBase64String(keyBytes);
        
        byte[] key = Convert.FromBase64String(encryptionKey);
        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.Key = key;
            byte[] iv = new byte[aesAlg.BlockSize / 8];
            using (FileStream fsInput = new FileStream(encryptedcsvFilePath, FileMode.Open))
            {
                fsInput.Read(iv, 0, iv.Length);
                aesAlg.IV = iv;

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                using (CryptoStream csDecrypt = new CryptoStream(fsInput, decryptor, CryptoStreamMode.Read))
                {
                    using (FileStream fsOutput = new FileStream(csvFilePath, FileMode.Create))
                    {
                        csDecrypt.CopyTo(fsOutput);
                    }
                }
            }
        }
        Debug.Log("복호화 success");
    }
    
}
