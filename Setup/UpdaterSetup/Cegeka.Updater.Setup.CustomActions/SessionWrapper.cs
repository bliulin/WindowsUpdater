using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Deployment.WindowsInstaller;
using Cegeka.Updater.Setup.CustomActions;

namespace Contracten.BaseCA
{
    public class SessionWrapper : ISessionWrapper
    {
        #region Instance fields

        private Session mSession;
        private bool mIsDeffered;
        #endregion

        #region Constructors

        public SessionWrapper(Session session)
        {
            if (null == session)
            {
                throw new ArgumentNullException("session");
            }

            mSession = session;
            InitializeSession();
        }

        #endregion

        #region >> ISessionWrapper Members

        public string Get(string propertyName)
        {
            if (!mIsDeffered)
            {
                mSession.Log(string.Format("{0} = {1}", propertyName, mSession[propertyName]));
                return mSession[propertyName];
            }

            if (!mSession.CustomActionData.ContainsKey(propertyName))
            {
                mSession.Log(string.Format("Key '{0}' was not found in the custom action data.", propertyName));
                return string.Empty;
            }

            string value = getStringFromBase64(mSession.CustomActionData[propertyName]);
            mSession.Log(string.Format("(DEFFERED) {0} = {1}", propertyName, value));

            return value;
        }

        public void Log(string msg)
        {
            mSession.Log(msg);
        }

        public void Set(string propertyName, string value)
        {
            if (mIsDeffered)
            {
                throw new InvalidOperationException("Cannot update session in deffered action.");
            }

            mSession[propertyName] = value;
        }

        #endregion

        private void InitializeSession()
        {
            try
            {
                var db = mSession.Database;
                mIsDeffered = false;
            }
            catch
            {
                mIsDeffered = true;
            }
        }

        private string getStringFromBase64(string base64)
        {
            return Encoding.UTF8.GetString(Convert.FromBase64String(base64));
        }
    }
}


