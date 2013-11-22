using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Net;
using System.IO;
using System.Web.Script.Serialization;

namespace Accela.Shared.AuthLibrary
{
    public static class AuthLibrary
    {
        private const string AuthenticationURL = @"https://auth.accela.com/oauth2/authorize";
        private const string TokenExchangeURL = @"https://apis.accela.com/oauth2/token";
        private const string GetUserUrl = @"https://apis.accela.com/v3/users/me";
        private const string AuthenticationRequest = @"{authUrl}?client_id={appId}&agency_name={agency}&environment={agencyEnvironment}&redirect_uri={redirectURI}&scope={scope}&response_type=code";
        private const string CodeExchangeRequest = @"grant_type=authorization_code&client_id={appId}&client_secret={appSecret}&redirect_uri={redirectURI}&code={code}";

        private const string codeString = "code=";

        // Method redirects to Accela Authentication
        public static void Login(ApplicationInfo appInfo, string redirectUrl, string scope)
        {
            HttpContext.Current.Response.Redirect(GetAuthUrlForRedirect(appInfo, redirectUrl, scope));
        }

        // Call this to get a user profile. If no user profile is returned, user needs to login
        public static CurrentUserProfile GetCurrentUserProfile(string redirectUrl, ApplicationInfo appInfo)
        {
            string url = HttpContext.Current.Request.Url.ToString();

            if (url.Contains(codeString))
            {
                // exchange for token
                int codePosition = url.IndexOf(codeString);
                string[] info = url.Substring(codePosition + codeString.Length).Split('&');
                appInfo.agencyName = info[1].Split('=')[1];
                appInfo.agencyEnvironment = info[2].Split('=')[1];
                Token token = GetToken(appInfo, redirectUrl, info[0]);

                // get user profile
                UserProfile userProfile = GetUserProfile(appInfo, token.access_token);
                CurrentUserProfile currentUserProfile = new CurrentUserProfile
                {
                    userProfile = userProfile,
                    applicationInfo = appInfo,
                    token = token
                };
                return currentUserProfile;
            }
            return null;
        }

        #region private methods

        private static string GetAuthUrlForRedirect(ApplicationInfo appInfo, string redirectUrl, string scope)
        {
            ValidateApplicationInfo(appInfo);
            if (string.IsNullOrEmpty(redirectUrl))
                throw new Exception("Please provide a valid url to redirect on authentication");
            if (string.IsNullOrEmpty(scope))
                throw new Exception("Please provide a valid scope for authentication");

            string requestString = AuthenticationRequest;
            requestString = requestString.Replace("{authUrl}", AuthenticationURL);
            requestString = requestString.Replace("{appId}", appInfo.applicationId);
            requestString = requestString.Replace("{agency}", appInfo.agencyName);
            requestString = requestString.Replace("{agencyEnvironment}", appInfo.agencyEnvironment);
            requestString = requestString.Replace("{redirectURI}", redirectUrl);
            requestString = requestString.Replace("{scope}", scope);

            return requestString;
        }

        private static Token GetToken(ApplicationInfo appInfo, string redirectUrl, string code)
        {
            ValidateApplicationInfo(appInfo);
            if (string.IsNullOrEmpty(redirectUrl))
                throw new Exception("Please provide a valid url to redirect on authentication");
            if (string.IsNullOrEmpty(code))
                throw new Exception("Please provide a valid code for authentication");

            Token token = new Token();
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(TokenExchangeURL);
            request.ContentType = "application/x-www-form-urlencoded";
            request.Headers.Add("x-accela-appid", appInfo.applicationId);

            string json = CodeExchangeRequest;
            json = json.Replace("{appId}", appInfo.applicationId);
            json = json.Replace("{appSecret}", appInfo.applicationSecret);
            json = json.Replace("{redirectURI}", redirectUrl);
            json = json.Replace("{code}", code);

            token = (Token)SendPostRequest(request, json, token);
            return token;
        }

        private static UserProfile GetUserProfile(ApplicationInfo appInfo, string token)
        {
            try
            {
                ValidateApplicationInfo(appInfo);
                UserProfile userProfile = new UserProfile();

                // get user profile
                string requestString = GetUserUrl;
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(requestString);
                userProfile = (UserProfile)SendGetRequest(request, userProfile, token, appInfo);
                return userProfile;
            }
            catch (WebException webException)
            {
                throw new Exception(HandleWebException(webException, " Error in Get User Profile :"));
            }
            catch (Exception exception)
            {
                throw new Exception("Error in Get User Profile : " + exception.Message);
            }
        }

        private static void ValidateApplicationInfo(ApplicationInfo appInfo)
        {
            if (appInfo == null)
                throw new Exception("Please provide valid Application Information.");
            if (appInfo.applicationId == null)
                throw new Exception("Please provide valid Application Id.");
            if (appInfo.applicationSecret == null)
                throw new Exception("Please provide valid Application Secret.");
            if (appInfo.applicationType == null)
                throw new Exception("Please provide valid Application Type.");
            if (appInfo.agencyName == null)
                throw new Exception("Please provide valid Agency Name.");
            if (appInfo.agencyEnvironment == null)
                throw new Exception("Please provide valid Environment.");
        }

        private static Object SendPostRequest(HttpWebRequest request, string requestString, Object response)
        {
            // Prepare Header
            request.Method = "POST";

            // Send
            using (StreamWriter s = new StreamWriter(request.GetRequestStream()))
            {
                s.Write(requestString);
                s.Flush();
            }

            // Receive
            var httpResponse = (HttpWebResponse)request.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd();
                response = new JavaScriptSerializer().Deserialize(result, response.GetType());
            }

            return response;
        }

        private static Object SendGetRequest(HttpWebRequest request, Object response, string token, ApplicationInfo appInfo)
        {
            // Prepare Header
            request.Method = "GET";
            request.ContentType = "application/json";
            request.Accept = "application/json";
            request.Headers.Add("x-accela-appid", appInfo.applicationId);
            request.Headers.Add("Authorization", token);
            request.Headers.Add("x-accela-secret", appInfo.applicationSecret);
            request.Headers.Add("x-accela-agency", appInfo.agencyName);
            request.Headers.Add("x-accela-environment", appInfo.agencyEnvironment);

            // Receive
            var httpResponse = (HttpWebResponse)request.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd();
                response = new JavaScriptSerializer().Deserialize(result, response.GetType());
            }

            return response;
        }

        private static string HandleWebException(WebException webException, string message)
        {
            message += " " + webException.Response.Headers["x-accela-resp-message"] + " Trace Id : " + webException.Response.Headers["x-accela-traceId"];
            return message;
        }
        #endregion
    }
}
