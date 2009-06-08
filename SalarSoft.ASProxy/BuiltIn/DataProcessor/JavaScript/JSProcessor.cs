using System;
using System.Collections.Generic;
using System.Text;
using SalarSoft.ASProxy.Exposed;

namespace SalarSoft.ASProxy.BuiltIn
{
    public class JSProcessor : ExJSProcessor
    {
        #region variables
        #endregion

        #region properties
        #endregion

        #region public methods
        public override string Execute()
        {
            Encoding _encoding;
            string resultCodes = StringStream.GetString(WebData.ResponseData,
                                    UserOptions.ForceEncoding,
                                    out _encoding);
            ContentEncoding = _encoding;

            IWebData webData = (IWebData)WebData;

            // Execute the result
            Execute(ref resultCodes,
                webData.ResponseInfo.ResponseUrl,
                webData.ResponseInfo.ResponseRootUrl);

            // the result
            return resultCodes;
        }

        public override void Execute(ref string codes, string pageUrl, string rootUrl)
        {
            try
            {
                if (UserOptions.Scripts)
                {
                    JSReplacer.AddEncoderMethodToPropertySet(ref codes, 
                        "location.href", 
                        Consts.ClientContent.JSEncoder_ASProxyEncoderMethodName);

                    JSReplacer.AddEncoderMethodToPropertySet(ref codes, 
                        "window.location",
                        Consts.ClientContent.JSEncoder_ASProxyEncoderMethodName);
                    JSReplacer.AddEncoderMethodToPropertySet(ref codes, 
                        "document.location",
                        Consts.ClientContent.JSEncoder_ASProxyEncoderMethodName);

                    JSReplacer.AddEncoderMethodToPropertySet(ref codes, 
                        "document.cookie",
                        Consts.ClientContent.JSEncoder_ASProxySetCookieMethodName);

                    // Cookie get, Since v5.0
                    JSReplacer.AddEncoderMethodToPropertyGet(ref codes, 
                        "document.cookie",
                        Consts.ClientContent.JSEncoder_ASProxyGetCookieMethodName);

                    // fisrt test location with base objects
                    JSReplacer.AddEncoderMethodToPropertyGet(ref codes, 
                        new string[] { "window", "document", "location" }
                        , new string[] { "location", "URL" },
                        Consts.ClientContent.JSEncoder_ASProxyWindowLocOverrider, 
                        false);

                    // then location attributes test 
                    JSReplacer.AddEncoderMethodToPropertyGetFirstPart(ref codes, 
                        new string[] { "location" }
                        , new string[] { "href", "search", "hash", "host", "hostname", "pathname", "port", "protocol", "replace", "assign" },
                        Consts.ClientContent.JSEncoder_ASProxyWindowLocOverrider, 
                        false);

                    JSReplacer.AddEncoderMethodToMethodFirstParameter(ref codes, 
                        "location.replace",
                        Consts.ClientContent.JSEncoder_ASProxyEncoderMethodName);

                    // It is not common to use "open" method directly.
                    // So i ignore to handle it here
                    //JSReplacer.AddEncoderMethodToMethodFirstParameter(ref html, "open", Consts.JSEncoder_ASProxyEncoderMethodName);

                    // Don't proccess these
                    // The "open" method will proccess in JavaScript encoder in "asproxyencoder.js" file.
                    // Since v4.8
                    //JSReplacer.AddEncoderMethodToMethodFirstParameter(ref html, "window.open", Consts.JSEncoder_ASProxyEncoderMethodName);
                    //JSReplacer.AddEncoderMethodToMethodFirstParameter(ref html, "location.open", Consts.JSEncoder_ASProxyEncoderMethodName);
                }

            }
            catch (Exception ex)
            {
                // error logs
                if (Systems.LogSystem.ErrorLogEnabled)
                    Systems.LogSystem.LogError(ex, pageUrl);
                
                codes = "/* ASProxy has some errors! \n"
                    + ex.Message + " */"
                    + codes;
            }
        }
        #endregion
    }
}
