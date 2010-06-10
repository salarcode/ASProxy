using System;
using System.Collections.Specialized;
using System.Xml;
using System.Web;
using MaxMind;
using System.Net;
using System.IO;

namespace ASProxy.Plugin.CountryBlocker
{
	public class BlockedCountries
	{
		private static StringCollection _blocked = new StringCollection();
		private static StringCollection _allowed = new StringCollection();
		private static GeoIPCountry geoIPCountry;

		public static StringCollection BlockedList
		{
			get { return _blocked; }
		}
		public static StringCollection AllowedList
		{
			get { return _allowed; }
		}

		internal static void Initialize()
		{
			//_blocked ;
			//_allowed ;

			ReadCountries();

			if (geoIPCountry == null)
				geoIPCountry = new GeoIPCountry(CountryDataFile);
		}

		internal static bool IsIpBlocked(string ip)
		{
			try
			{
				IPAddress address = IPAddress.Parse(ip);
				string code = geoIPCountry.GetCountryCode(address);
				if (code != null)
					return IsCountryBlocked(code);

				return false;
			}
			catch (Exception)
			{
				return false;
			}
		}

		internal static bool IsCountryBlocked(string countryCode)
		{
			if (_allowed.Count > 0)
				return !IsAllowed(countryCode);

			return IsBlocked(countryCode);
		}

		internal static bool IsAllowed(string countryCode)
		{
			return _allowed.Contains(countryCode);
		}

		internal static bool IsBlocked(string countryCode)
		{
			return _blocked.Contains(countryCode);
		}

		private static void ReadCountries()
		{
			XmlDocument xmlDoc = new XmlDocument();
			try
			{
				xmlDoc.Load(XmlCountryFile);
				XmlNode rootNode = xmlDoc.SelectSingleNode("CountryBlocker");

				ReadCountryList(rootNode.SelectSingleNode("blocked"), _blocked);
				ReadCountryList(rootNode.SelectSingleNode("allowed"), _allowed);
			}
			catch (Exception)
			{ }
		}

		public static void SaveCountries()
		{
			XmlDocument xmlDoc = new XmlDocument();
			try
			{
				xmlDoc.LoadXml("<?xml version='1.0' encoding='utf-8' ?><CountryBlocker></CountryBlocker>");
				XmlNode rootNode = xmlDoc.SelectSingleNode("CountryBlocker");

				SaveToXmlDoc(xmlDoc, _blocked, rootNode, "blocked");
				SaveToXmlDoc(xmlDoc, _allowed, rootNode, "allowed");

				Directory.CreateDirectory(Path.GetDirectoryName(XmlCountryFile));
				xmlDoc.Save(XmlCountryFile);
			}
			catch (Exception)
			{ }
		}

		private static void SaveToXmlDoc(XmlDocument xmlDoc, StringCollection list, XmlNode rootNode, string nodeName)
		{
			XmlNode node = xmlDoc.CreateElement(nodeName);
			foreach (string item in list)
			{
				XmlNode countryCode = xmlDoc.CreateElement("country");

				XmlAttribute attribute = xmlDoc.CreateAttribute("code");
				attribute.Value = item;
				countryCode.Attributes.Append(attribute);

				node.AppendChild(countryCode);

			}
			// add to the parent
			rootNode.AppendChild(node);
		}

		private static void ReadCountryList(XmlNode rootNode, StringCollection coll)
		{
			foreach (XmlNode node in rootNode)
			{
				if (node.NodeType == XmlNodeType.Comment)
					continue;

				coll.Add(node.Attributes["code"].Value.ToUpper());
			}
		}

		private static string XmlCountryFile
		{
			get { return HttpContext.Current.Server.MapPath("~/App_Data/CountryBlocker/CountryList.xml"); }
		}
		private static string CountryDataFile
		{
			get { return HttpContext.Current.Server.MapPath("~/App_Data/CountryBlocker/GeoIP.dat"); }
		}


		public static readonly string[] CountryCodes = { 
			"AP","EU","AD","AE","AF","AG","AI","AL","AM","AN","AO","AQ","AR","AS",
			"AT","AU","AW","AZ","BA","BB","BD","BE","BF","BG","BH","BI","BJ","BM","BN",
			"BO","BR","BS","BT","BV","BW","BY","BZ","CA","CC","CD","CF","CG","CH","CI",
			"CK","CL","CM","CN","CO","CR","CU","CV","CX","CY","CZ","DE","DJ","DK","DM",
			"DO","DZ","EC","EE","EG","EH","ER","ES","ET","FI","FJ","FK","FM","FO","FR",
			"FX","GA","GB","GD","GE","GF","GH","GI","GL","GM","GN","GP","GQ","GR","GS",
			"GT","GU","GW","GY","HK","HM","HN","HR","HT","HU","ID","IE","IL","IN","IO",
			"IQ","IR","IS","IT","JM","JO","JP","KE","KG","KH","KI","KM","KN","KP","KR",
			"KW","KY","KZ","LA","LB","LC","LI","LK","LR","LS","LT","LU","LV","LY","MA",
			"MC","MD","MG","MH","MK","ML","MM","MN","MO","MP","MQ","MR","MS","MT","MU",
			"MV","MW","MX","MY","MZ","NA","NC","NE","NF","NG","NI","NL","NO","NP","NR",
			"NU","NZ","OM","PA","PE","PF","PG","PH","PK","PL","PM","PN","PR","PS","PT",
			"PW","PY","QA","RE","RO","RU","RW","SA","SB","SC","SD","SE","SG","SH","SI",
			"SJ","SK","SL","SM","SN","SO","SR","ST","SV","SY","SZ","TC","TD","TF","TG",
			"TH","TJ","TK","TM","TN","TO","TL","TR","TT","TV","TW","TZ","UA","UG","UM",
			"US","UY","UZ","VA","VC","VE","VG","VI","VN","VU","WF","WS","YE","YT","RS",
			"ZA","ZM","ME","ZW","A1","A2","O1","AX","GG","IM","JE","BL","MF"
		};

		public static readonly string[] CountryNames = {
			"Asia/Pacific Region","Europe","Andorra","United Arab Emirates","Afghanistan",
			"Antigua and Barbuda","Anguilla","Albania","Armenia","Netherlands Antilles","Angola",
			"Antarctica","Argentina","American Samoa","Austria","Australia","Aruba","Azerbaijan",
			"Bosnia and Herzegovina","Barbados","Bangladesh","Belgium","Burkina Faso","Bulgaria",
			"Bahrain","Burundi","Benin","Bermuda","Brunei Darussalam","Bolivia","Brazil","Bahamas",
			"Bhutan","Bouvet Island","Botswana","Belarus","Belize","Canada","Cocos (Keeling) Islands",
			"Congo, The Democratic Republic of the","Central African Republic","Congo","Switzerland",
			"Cote D'Ivoire","Cook Islands","Chile","Cameroon","China","Colombia","Costa Rica","Cuba",
			"Cape Verde","Christmas Island","Cyprus","Czech Republic","Germany","Djibouti","Denmark",
			"Dominica","Dominican Republic","Algeria","Ecuador","Estonia","Egypt","Western Sahara",
			"Eritrea","Spain","Ethiopia","Finland","Fiji","Falkland Islands (Malvinas)",
			"Micronesia, Federated States of","Faroe Islands","France","France, Metropolitan","Gabon",
			"United Kingdom","Grenada","Georgia","French Guiana","Ghana","Gibraltar","Greenland",
			"Gambia","Guinea","Guadeloupe","Equatorial Guinea","Greece",
			"South Georgia and the South Sandwich Islands","Guatemala","Guam","Guinea-Bissau","Guyana",
			"Hong Kong","Heard Island and McDonald Islands","Honduras","Croatia","Haiti","Hungary",
			"Indonesia","Ireland","Israel","India","British Indian Ocean Territory","Iraq",
			"Iran, Islamic Republic of","Iceland","Italy","Jamaica","Jordan","Japan","Kenya",
			"Kyrgyzstan","Cambodia","Kiribati","Comoros","Saint Kitts and Nevis",
			"Korea, Democratic People's Republic of","Korea, Republic of","Kuwait","Cayman Islands",
			"Kazakstan","Lao People's Democratic Republic","Lebanon","Saint Lucia","Liechtenstein",
			"Sri Lanka","Liberia","Lesotho","Lithuania","Luxembourg","Latvia","Libyan Arab Jamahiriya",
			"Morocco","Monaco","Moldova, Republic of","Madagascar","Marshall Islands","Macedonia",
			"Mali","Myanmar","Mongolia","Macau","Northern Mariana Islands","Martinique","Mauritania",
			"Montserrat","Malta","Mauritius","Maldives","Malawi","Mexico","Malaysia","Mozambique",
			"Namibia","New Caledonia","Niger","Norfolk Island","Nigeria","Nicaragua","Netherlands",
			"Norway","Nepal","Nauru","Niue","New Zealand","Oman","Panama","Peru","French Polynesia",
			"Papua New Guinea","Philippines","Pakistan","Poland","Saint Pierre and Miquelon",
			"Pitcairn Islands","Puerto Rico","Palestinian Territory","Portugal","Palau","Paraguay",
			"Qatar","Reunion","Romania","Russian Federation","Rwanda","Saudi Arabia",
			"Solomon Islands","Seychelles","Sudan","Sweden","Singapore","Saint Helena","Slovenia",
			"Svalbard and Jan Mayen","Slovakia","Sierra Leone","San Marino","Senegal","Somalia",
			"Suriname","Sao Tome and Principe","El Salvador","Syrian Arab Republic","Swaziland",
			"Turks and Caicos Islands","Chad","French Southern Territories","Togo","Thailand",
			"Tajikistan","Tokelau","Turkmenistan","Tunisia","Tonga","Timor-Leste","Turkey",
			"Trinidad and Tobago","Tuvalu","Taiwan","Tanzania, United Republic of","Ukraine","Uganda",
			"United States Minor Outlying Islands","United States","Uruguay","Uzbekistan",
			"Holy See (Vatican City State)","Saint Vincent and the Grenadines","Venezuela",
			"Virgin Islands, British","Virgin Islands, U.S.","Vietnam","Vanuatu","Wallis and Futuna",
			"Samoa","Yemen","Mayotte","Serbia","South Africa","Zambia","Montenegro","Zimbabwe",
			"Anonymous Proxy","Satellite Provider","Other","Aland Islands","Guernsey","Isle of Man",
			"Jersey","Saint Barthelemy","Saint Martin"
		};

	}
}
