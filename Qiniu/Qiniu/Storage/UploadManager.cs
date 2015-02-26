﻿using Qiniu.Common;
using Qiniu.Http;
using Qiniu.Storage.Persistent;
using System;
using System.IO;
using System.IO.IsolatedStorage;

namespace Qiniu.Storage
{
    public class UploadManager
    {
        private HttpManager httpManager;
        private ResumeRecorder resumeRecorder;
        private KeyGenerator keyGenerator;

        public UploadManager()
        {
            this.httpManager = new HttpManager();
            this.resumeRecorder = null;
            this.keyGenerator = null;
        }

        public UploadManager(HttpManager httpManager)
        {
            this.httpManager = httpManager;
            this.resumeRecorder = null;
            this.keyGenerator = null;
        }

        public UploadManager(ResumeRecorder recorder, KeyGenerator generator)
        {
            this.httpManager = new HttpManager();
            this.resumeRecorder = recorder;
            this.keyGenerator = generator;
        }

        public UploadManager(HttpManager httpManager, ResumeRecorder recorder, KeyGenerator generator)
        {
            this.httpManager = httpManager;
            this.resumeRecorder = recorder;
            this.keyGenerator = generator;
        }

        public void uploadData(byte[] data, string key,
            string token, UploadOptions uploadOptions, UpCompletionHandler upCompletionHandler)
        {
            new FormUploader().uploadData(this.httpManager, data, key, token, uploadOptions, upCompletionHandler);
        }

        public void uploadStream(Stream stream, string key, string token,
            UploadOptions uploadOptions, UpCompletionHandler upCompletionHandler)
        {
            new FormUploader().uploadStream(this.httpManager, stream, key, token, uploadOptions, upCompletionHandler);
        }

        public void uploadFile(string filePath, string key, string token,
            UploadOptions uploadOptions, UpCompletionHandler upCompletionHandler)
        {
            try
            {
                long fileSize = 0;
                using (IsolatedStorageFileStream s = new IsolatedStorageFileStream(filePath, FileMode.Open,
                    IsolatedStorageFile.GetUserStoreForApplication()))
                {
                    fileSize = s.Length;
                }
                //check
                if (fileSize <= Config.PUT_THRESHOLD)
                {
                    new FormUploader().uploadFile(this.httpManager, filePath, key, token, uploadOptions, upCompletionHandler);
                }
                else
                {
                    new ResumeUploader(this.httpManager, this.resumeRecorder, this.keyGenerator(), filePath, key, token, uploadOptions, upCompletionHandler);
                }
            }
            catch (Exception ex)
            {
                upCompletionHandler(key, ResponseInfo.fileError(ex), null);
            }
        }
    }
}
