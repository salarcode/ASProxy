﻿using System.Web;
using System;
using System.Reflection;
using System.Xml;
namespace SalarSoft.ASProxy
{
    ///<summary>
    /// User defined data processing options 
    ///</summary>
    public struct UserOptions
    {
        public bool Cookies;
        public bool Images;
        public bool Scripts;
        public bool Links;
        public bool IFrame;
        public bool FrameSet;
        public bool SubmitForms;
        public bool CssLink;
        public bool EmbedObjects;
        public bool HttpCompression;
        public bool EncodeUrl;
        public bool OrginalUrl;
        public bool PageTitle;
        public bool ForceEncoding;

        /// <summary>
        /// Specifies scripts should strip or not. This option disabled by default.
        /// </summary>
        public bool RemoveScripts;
        public bool RemoveImages;

        /// <summary>
        /// Inherits DOCTYPE.
        /// </summary>
        public bool DocType;

        /// <summary>
        /// Initialize a new instance of UserOptions with default value
        /// </summary>
        public static UserOptions LoadDefaults(bool def)
        {
            UserOptions result;
            result.HttpCompression = false;		// Disabled by default
            result.ForceEncoding = false;	// Disabled by default
            result.RemoveScripts = false;		// Disabled by default
            result.RemoveImages = false;			// Disabled by default
            result.DocType = def;             // Disabled by default
            result.OrginalUrl = def;
            result.Images = def;
            result.Scripts = def;
            result.Links = def;
            result.IFrame = def;
            result.FrameSet = def;
            result.SubmitForms = def;
            result.CssLink = def;
            result.EncodeUrl = def;
            result.EmbedObjects = def;
            result.Cookies = def;
            result.PageTitle = def;
            return result;
        }

        public void SaveToResponse()
        {
            HttpContext context = HttpContext.Current;

            HttpCookie cookie = new HttpCookie(Consts.FrontEndPresentation.CookieMasterName);
            cookie.Expires = DateTime.Now.AddMonths(1);

            Type type = typeof(UserOptions);
            foreach (FieldInfo info in type.GetFields())
            {
                if ((bool)info.GetValue(this))
                    cookie[info.Name] = "true";
                else
                    cookie[info.Name] = "false";
            }
            context.Response.Cookies.Add(cookie);
        }

        public static UserOptions ReadFromRequest()
        {
            UserOptions result = LoadDefaults(true);

            HttpContext context = HttpContext.Current;
            HttpCookie cookie = context.Request.Cookies[Consts.FrontEndPresentation.CookieMasterName];
            if (cookie == null)
                return result;

            Type type = typeof(UserOptions);
            object instance = (object)result;

            foreach (FieldInfo info in type.GetFields())
            {
                string cookievalue = cookie[info.Name];

                // cookie has value?
                if (!string.IsNullOrEmpty(cookievalue))
                {
                    try
                    {
                        info.SetValue(instance, Convert.ToBoolean(cookievalue));
                    }
                    catch
                    {
                        info.SetValue(instance, true);
                    }
                }
            }

            result = (UserOptions)instance;

            return result;
        }

        public static UserOptions ReadFromXml(string xmlFile)
        {
            UserOptions result = LoadDefaults(true);

            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(xmlFile);
                XmlNode baseNode = xmlDoc.SelectSingleNode("Options");

                Type optionType = typeof(UserOptions);

                foreach (XmlNode node in baseNode.ChildNodes)
                {
                    FieldInfo info = optionType.GetField(node.Name);
                    if (info.FieldType.IsEnum)
                        info.SetValue(result, Enum.Parse(info.FieldType, node.InnerText));
                    else
                        info.SetValue(result, Convert.ChangeType(node.InnerText, info.FieldType));
                }
            }
            catch { }

            return result;
        }

        public void SaveToXml(string xmlFile)
        {
            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml("<?xml version='1.0' encoding='utf-8' ?><Options></Options>");

                XmlNode baseNode = xmlDoc.SelectSingleNode("Options");
                Type optionType = typeof(UserOptions);

                foreach (FieldInfo info in optionType.GetFields(BindingFlags.Instance | BindingFlags.Public))
                {
                    XmlNode node = xmlDoc.CreateElement(info.Name);
                    node.InnerText = info.GetValue(this).ToString();
                    baseNode.AppendChild(node);
                }

                xmlDoc.Save(xmlFile);
            }
            catch { }
        }
    }
}