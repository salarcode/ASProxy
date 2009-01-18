
namespace SalarSoft.ASProxy
{
	public class JSReplacer
	{
		private const string _JSMethodStart = "(";
		private const string _JSMethodEnd = ")";


		/// <summary>
		/// Finds and encode codes within "script" tag.
		/// </summary>
		public static void ReplaceScriptTagCodes(ref string htmlCode)
		{
			const string scriptStart = "<script";
			const string scriptEnd = "</script>";
			const string htmlTagEnd = ">";
			TextRange position;
			int index = 0;

			do
			{
				string scriptContent = "";

				// Find script tag position start
				position.Start = Performance.IndexOfIgnoreCase(ref htmlCode, scriptStart, index);
				if (position.Start == -1)
					break;

				// locate tag end
				position.Start = Performance.IndexOfMatchCase(ref htmlCode, htmlTagEnd, position.Start);
				if (position.Start == -1)
					break;
				position.Start++;

				// Locate end tag
				position.End = Performance.IndexOfIgnoreCase(ref htmlCode, scriptEnd, position.Start);
				if (position.End == -1)
					break;

				// Set next searching start position
				index = position.End;

				// Get the content
				scriptContent = htmlCode.Substring(position.Start, position.End - position.Start);

				// If there is nothing, go to next tag
				if (scriptContent.Trim().Length == 0)
					continue;


				// Very bad use of processor, But there is no other way to do!
				// Create a new instance of processor
				JSProcessor processor = new JSProcessor(null);

				// Process the script content
				scriptContent = processor.Execute(scriptContent);


				// Replace the new style
				htmlCode = htmlCode.Remove(position.Start, position.End - position.Start);
				htmlCode = htmlCode.Insert(position.Start, scriptContent);

			} while (index != -1);

		}

		/// <summary>
		/// Enclose property setter with an encoder method
		/// </summary>
		public static void AddEncoderMethodToPropertySet(ref string jsCode, string propertyFullName, string encoderMethodName)
		{
			string encodingMethod = encoderMethodName + _JSMethodStart;
			int index = 0;
			TextRange position;

			do
			{
				// Find property position
				position = JSParser.FindPropertySetterRange(ref jsCode, propertyFullName, index);

				// There is no more property
				if (position.Start == -1)
					break;

				// Next search
				index = position.Start;

				if (position.End > -1)
				{
					// Set next searching position
					index = position.End + encodingMethod.Length;

					// first method ending then method name
					jsCode = jsCode.Insert(position.End, _JSMethodEnd);
					jsCode = jsCode.Insert(position.Start, encodingMethod);
				}

			}
			while (index != -1);
		}

		/// <summary>
		/// Replaces property getter with an encoder method
		/// </summary>
		internal static void AddEncoderMethodToPropertyGet(ref string jsCode, string propertyFullName, string encoderMethodName)
		{
			string encodingMethod = encoderMethodName + _JSMethodStart + _JSMethodEnd;
			int index = 0;
			TextRange position;

			do
			{
				// Find property position
				position = JSParser.FindPropertyGetterRange(ref jsCode, propertyFullName, index);

				// There is no more property
				if (position.Start == -1)
					break;

				// Next search
				index = position.Start;

				if (position.End > -1)
				{
					// Set next searching position
					index = position.Start + encodingMethod.Length;

					// first remove previous value, then add new method
					jsCode = jsCode.Remove(position.Start, position.End - position.Start);
					jsCode = jsCode.Insert(position.Start, encodingMethod);
				}

			}
			while (index != -1);
		}

		/// <summary>
		/// Replaces property getter with an encoder method
		/// </summary>
		internal static void AddEncoderMethodToPropertyGet(ref string jsCode, string[] commandFirstBase, string[] commandSecondItem, string encoderMethodName, bool addMethodParenthesis)
		{
			string encodingMethod;
			if (addMethodParenthesis)
				encodingMethod = encoderMethodName + _JSMethodStart + _JSMethodEnd;
			else
				encodingMethod = encoderMethodName;

			int index = 0;
			TextRange position;

			do
			{
				// Find property position
				position = JSParser.FindPropertyGetterRange(ref jsCode, commandFirstBase, commandSecondItem, index);

				// There is no more property
				if (position.Start == -1)
					break;

				// Next search
				index = position.Start;

				if (position.End > -1)
				{
					// Set next searching position
					index = position.Start + encodingMethod.Length;

					// first remove previous value, then add new method
					jsCode = jsCode.Remove(position.Start, position.End - position.Start);
					jsCode = jsCode.Insert(position.Start, encodingMethod);
				}

			}
			while (index != -1);
		}

		/// <summary>
		/// Replaces property getter with an encoder method
		/// </summary>
		internal static void AddEncoderMethodToPropertyGetFirstPart(ref string jsCode, string[] commandFirstBase, string[] commandSecondItem, string encoderMethodName, bool addMethodParenthesis)
		{
			string encodingMethod;
			if (addMethodParenthesis)
				encodingMethod = encoderMethodName + _JSMethodStart + _JSMethodEnd;
			else
				encodingMethod = encoderMethodName;

			int index = 0;
			TextRange position;

			do
			{
				// Find property position
				position = JSParser.FindPropertyGetterFirstPartRange(ref jsCode, commandFirstBase, commandSecondItem, index);

				// There is no more property
				if (position.Start == -1)
					break;

				// Next search
				index = position.Start;

				if (position.End > -1)
				{
					// Set next searching position
					index = position.Start + encodingMethod.Length;

					// first remove previous value, then add new method
					jsCode = jsCode.Remove(position.Start, position.End - position.Start);
					jsCode = jsCode.Insert(position.Start, encodingMethod);
				}

			}
			while (index != -1);
		}

		/// <summary>
		/// Enclose method first parameter with an encoder method
		/// </summary>
		public static void AddEncoderMethodToMethodFirstParameter(ref string jsCode, string methodFullName, string encoderMethodName)
		{
			string encodingMethod = encoderMethodName + _JSMethodStart;
			int index = 0;
			TextRange position;

			do
			{
				// Find property position
				position = JSParser.FindMethodFirstParameterRange(ref jsCode, methodFullName, index);

				// There is no more property
				if (position.Start == -1)
					break;

				// Next search
				index = position.Start;

				if (position.End >= 0)
				{
					// Set next searching position
					index = position.End + encodingMethod.Length;

					// We should add method end then add method name
					jsCode = jsCode.Insert(position.End, _JSMethodEnd);
					jsCode = jsCode.Insert(position.Start, encodingMethod);
				}
			}
			while (index != -1);
		}


	}
}