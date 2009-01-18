using System;
using System.Text;

namespace SalarSoft.ASProxy
{

	/// <summary>
	/// Parse the javascript codes!
	/// </summary>
	public class JSParser
	{
		class Internal
		{
			/// <summary>
			/// Locates next non-white space character
			/// </summary>
			internal static int FindNextValidCharacter(ref string source, int searchingStartIndex)
			{
				for (int i = searchingStartIndex; i < source.Length; i++)
				{
					if (Char.IsWhiteSpace(source[i]) == false)
						return i;
				}
				return -1;
			}

			internal static int FindCloserIndexOfItems(ref string text, string[] items, int searchingStart, out string foundItem)
			{
				foundItem = "";
				int result = -1;
				for (int i = 0; i < items.Length; i++)
				{
					int index = Performance.IndexOfMatchCase(ref text, items[i], searchingStart);
					if (index != -1 && (result == -1 || index < result))
					{
						foundItem = items[i];
						result = index;
					}
				}
				return result;
			}

		}
		/// <summary>
		/// This function locates property setter location.
		/// </summary>
		/// <param name="source">Search place</param>
		/// <param name="propertyFullName">Name of property including dot for searching</param>
		/// <returns>All value</returns>
		public static TextRange FindPropertySetterRange(ref string source, string propertyFullName, int searchingStartIndex)
		{
			char propertyStart = '=';
			TextRange result = new TextRange(-1, -1);

			int searchIndex = searchingStartIndex;

			do
			{
				// Locate start position of property
				result = FindDottedCommandSetterStartPosition(ref source, propertyFullName, searchIndex, propertyStart);

				// If nothing found, go away.
				if (result.Start == -1)
					break;

				// Next search
				searchIndex = result.Start;

				// If something found
				if (result.End == 0)
				{
					// Locate end of property
					result.End = FindPropertySetEndRange(ref source, result.Start, source.Length);

					// If really something found, return result
					if (result.End != -1)
						break;

					// Sorry, Next search
					//searchIndex = result.End;
				}
			}
			while (true);// Extreme loop? No.

			return result;
		}

		/// <summary>
		/// Locates property getter location.
		/// </summary>
		public static TextRange FindPropertyGetterRange(ref string source, string propertyFullName, int searchingStartIndex)
		{
			char propertyStart = '=';
			TextRange result = new TextRange(-1, -1);

			int searchIndex = searchingStartIndex;

			do
			{
				// Locate start position of property
				result = FindDottedCommandGetterStartPosition(ref source, propertyFullName, searchIndex, propertyStart);

				// If nothing found, go away.
				if (result.Start == -1)
					break;

				// Next search
				searchIndex = result.Start;

				// If something found
				if (result.End > -1)
					break;
			}
			while (true);

			return result;
		}

		/// <summary>
		/// Locates property getter location.
		/// </summary>
		public static TextRange FindPropertyGetterRange(ref string source, string[] commandFirstBase, string[] commandSecondItem, int searchingStartIndex)
		{
			char propertyStart = '=';
			TextRange result = new TextRange(-1, -1);

			int searchIndex = searchingStartIndex;

			do
			{
				// Locate start position of property
				result = FindDottedCommandGetterStartPosition(ref source, commandFirstBase, commandSecondItem, searchIndex, propertyStart, false);

				// If nothing found, go away.
				if (result.Start == -1)
					break;

				// Next search
				searchIndex = result.Start;

				// If something found
				if (result.End > -1)
					break;
			}
			while (true);

			return result;
		}

		/// <summary>
		/// Locates property getter location.
		/// </summary>
		public static TextRange FindPropertyGetterFirstPartRange(ref string source, string[] commandFirstBase, string[] commandSecondItem, int searchingStartIndex)
		{
			char propertyStart = '=';
			TextRange result = new TextRange(-1, -1);

			int searchIndex = searchingStartIndex;

			do
			{
				// Locate start position of property
				result = FindDottedCommandGetterStartPosition(ref source, commandFirstBase, commandSecondItem, searchIndex, propertyStart, true);

				// If nothing found, go away.
				if (result.Start == -1)
					break;

				// Next search
				searchIndex = result.Start;

				// If something found
				if (result.End > -1)
					break;
			}
			while (true);

			return result;
		}


		/// <summary>
		/// This function locates hole text (including parameters and etc.) between start parenthesis and end parenthesis of method.
		/// </summary>
		/// <param name="source">Search place</param>
		/// <param name="methodFullName">Name of method including dot for searching</param>
		/// <returns>All parameters</returns>
		public static TextRange FindMethodParametersRange(ref string source, string methodFullName, int searchingStartIndex)
		{
			char methodStart = '(';
			TextRange result = new TextRange(-1, -1);

			int searchIndex = searchingStartIndex;

			do
			{
				// Locate start position of property
				result = FindDottedCommandSetterStartPosition(ref source, methodFullName, searchIndex, methodStart);

				// If nothing found, go away.
				if (result.Start == -1)
					break;

				// Next search
				searchIndex = result.Start;

				// If something found
				if (result.End == 0)
				{
					// Locate end of method
					result.End = FindMethodHoleEndRange(ref source, result.Start, source.Length);

					// If really something found, return result
					if (result.End != -1)
						break;

					// Sorry, Next search
					//searchIndex = result.End;
				}
			}
			while (true);// Extreme loop? No.

			return result;
		}


		/// <summary>
		/// Find the first parameter of a method
		/// </summary>
		/// <param name="source">Search place</param>
		/// <param name="methodFullName">Name of method including dot for searching</param>
		/// <returns>method parameter value position</returns>
		public static TextRange FindMethodFirstParameterRange(ref string source, string methodFullName, int searchingStartIndex)
		{
			char methodStart = '(';
			TextRange result = new TextRange(-1, -1);

			int searchIndex = searchingStartIndex;

			do
			{
				// Locate start position of method
				result = FindDottedCommandSetterStartPosition(ref source, methodFullName, searchIndex, methodStart);

				// If nothing found, go away.
				if (result.Start == -1)
					break;

				// Next search
				searchIndex = result.Start;

				// If something found
				if (result.End == 0)
				{
					// Locate method
					result.End = FindMethodParameterEndRange(ref source, result.Start, source.Length, true);

					// If really something found, return result
					if (result.End != -1)
						break;

					// Sorry, Next search
					//searchIndex = result.End;
				}
			}
			while (true);// Extreme loop? No.

			return result;
		}



		#region core functions

		/// <summary>
		/// Location start postiion of any dotted command or nodot command. ex. window.location='salarsoft';
		/// </summary>
		/// <param name="source">Source of codes provided for searching</param>
		/// <param name="commandFullName">Name of command including dots</param>
		/// <param name="searchingStartIndex">Searching start position</param>
		/// <returns>Start position of command, and status of success</returns>
		private static TextRange FindDottedCommandSetterStartPosition(ref string source, string commandFullName, int searchingStartIndex, char commandStartCharacter)
		{
			TextRange result = new TextRange(-1, -1);
			char dot = '.';
			int partSearchStartIndex = searchingStartIndex;


			// Split full name of property using dot. 
			string[] commandParts = commandFullName.Split(new char[] { dot }, StringSplitOptions.RemoveEmptyEntries);


			// locate property parts
			for (int i = 0; i < commandParts.Length; i++)
			{
				string commandPart = commandParts[i];
				int partIndex;
				//--------------------------------------------------
				// locate current part position with match case searching
				partIndex = Performance.IndexOfMatchCase(ref source, commandPart, partSearchStartIndex);

				// One part of property does not exists, so we should run away!
				if (partIndex == -1)
				{
					// if this is first part, is seems there isn't any more of this
					// else return last found position
					if (i == 0)
						result.Start = -1;
					else
						result.Start = partSearchStartIndex;

					// This negative result show's that the process failed
					result.End = -1;
					return result;
				}


				//--------------------------------------------------
				// Test for free spaces after first part of name only
				if (i > 0)
				{
					// Test for free space between equal mark and semicolon (ex. window.location = 'text' )
					string shouldBeFreeSpace = source.Substring(partSearchStartIndex, partIndex - partSearchStartIndex);

					// Oops! this isn't our victim!
					if (shouldBeFreeSpace.Trim().Length > 0)
					{
						if (i == 0)
							result.Start = -1;
						else
							result.Start = partSearchStartIndex;
						result.End = -1;
						return result;
					}
				}

				// Set part position and its length to result
				result.Start = partIndex + commandPart.Length;

				//--------------------------------------------------
				// If this isn't last part, we should locate next dot
				if (i < commandParts.Length - 1)
				{
					int dotPos;

					// Locate dot position after a part
					dotPos = Performance.IndexOfMatchCase(ref source, dot, result.Start);

					// There is not dot around here?!!
					if (dotPos == -1)
					{
						// result.Start = result.Start; // Same
						result.End = -1;
						return result;
					}

					// Test for free space between equal mark and semicolon (ex. window.location ('text'))
					string shouldBeFreeSpace = source.Substring(result.Start, dotPos - result.Start);

					// Oops! this isn't our victim!
					if (shouldBeFreeSpace.Trim().Length > 0)
					{
						// result.Start = result.Start;  // Same
						result.End = -1;
						return result;
					}

					// Set last position to dot position
					result.Start = dotPos + 1;
					partSearchStartIndex = result.Start;
				}

			}

			// if something found
			if (result.Start != -1)
			{
				int startCharPos = Performance.IndexOfMatchCase(ref source, commandStartCharacter, result.Start);

				// Is there any commandStartCharacter??
				if (startCharPos == -1)
				{
					// result.Start = result.Start; // Same
					result.End = -1;
					return result;
				}

				// Test for free space between equal mark and semicolon (ex. window.location ('text'))
				string shouldBeFreeSpace = source.Substring(result.Start, startCharPos - result.Start);

				// Oops! this isn't our victim!
				if (shouldBeFreeSpace.Trim().Length > 0)
				{
					// result.Start = result.Start; // Same
					result.End = -1;
					return result;
				}

				// Oops! This is a condition expression!
				if (commandStartCharacter == '=' && source[result.Start + 1] == '=')
				{
					result.End = -1;
					return result;
				}

				// Set found position to result
				result.Start = startCharPos + commandStartCharacter.ToString().Length;

				// The result is successfull 
				result.End = 0;
			}

			return result;
		}

		private static TextRange FindDottedCommandGetterStartPosition(ref string source, string commandFullName, int searchingStartIndex, char shouldNotStartWithChar)
		{
			TextRange result = new TextRange(-1, -1);
			char dot = '.';
			int partSearchStartIndex = searchingStartIndex;


			// Split full name of property using dot. 
			string[] commandParts = commandFullName.Split(new char[] { dot }, StringSplitOptions.RemoveEmptyEntries);

			int start = -1, end = -1;

			// locate property parts
			for (int i = 0; i < commandParts.Length; i++)
			{
				string commandPart = commandParts[i];
				int partIndex;

				//--------------------------------------------------
				// locate current part position with match case searching
				partIndex = Performance.IndexOfMatchCase(ref source, commandPart, partSearchStartIndex);

				// One part of property does not exists, so we should run away!
				if (partIndex == -1)
				{
					// if this is first part, is seems there isn't any more of this
					// else return last found position
					if (i == 0)
						result.Start = -1;
					else
						result.Start = partSearchStartIndex;

					// This negative result show's that the process failed
					result.End = -1;
					return result;
				}

				// saving start position
				if (i == 0)
					start = partIndex;

				//--------------------------------------------------
				// Test for free spaces after first part of name only
				if (i > 0)
				{
					// Test for free space between equal mark and semicolon (ex. window.location = 'text' )
					string shouldBeFreeSpace = source.Substring(partSearchStartIndex, partIndex - partSearchStartIndex);

					// Oops! this isn't our victim!
					if (shouldBeFreeSpace.Trim().Length > 0)
					{
						//result.Start = result.Start; //same
						if (i == 0)
							result.Start = -1;
						else
							result.Start = partSearchStartIndex;

						result.End = -1;
						return result;
					}
				}

				// add part length to result
				result.Start = partIndex + commandPart.Length;

				//--------------------------------------------------
				// If this isn't last part, we should locate next dot
				if (i < commandParts.Length - 1)
				{
					int dotPos;

					// Locate dot position after a part
					dotPos = Performance.IndexOfMatchCase(ref source, dot, result.Start);

					// There is not dot around here?!!
					if (dotPos == -1)
					{
						// result.Start = result.Start; // Same
						result.End = -1;
						return result;
					}

					// Test for free space between equal mark and semicolon (ex. window.location ('text'))
					string shouldBeFreeSpace = source.Substring(result.Start, dotPos - result.Start);

					// Oops! this isn't our victim!
					if (shouldBeFreeSpace.Trim().Length > 0)
					{
						// result.Start = result.Start;  // Same
						result.End = -1;
						return result;
					}

					// Set last position to dot position
					result.Start = dotPos + 1;
					partSearchStartIndex = result.Start;
				}

				// every time result.Start is end of search result
				end = result.Start;
			}

			// if something found
			if (result.Start != -1)
			{
				int nextChar = Internal.FindNextValidCharacter(ref source, result.Start);
				if (nextChar != -1)
				{

					// Oops! this isn't our victim!
					if (source[nextChar] == shouldNotStartWithChar)
					{
						result.End = -1;
						return result;
					}
				}


				// Set found position to result
				result.Start = start;

				// The result is successfull 
				result.End = end;
			}

			return result;
		}

		/// <summary>
		/// Locates a command getter start position. The command names is specified in two array.
		/// </summary>
		private static TextRange FindDottedCommandGetterStartPosition(ref string source, string[] commandFirstBase, string[] commandSecondItem, int searchingStartIndex, char shouldNotStartWithChar, bool returnOnlyFirstPart)
		{
			TextRange result = new TextRange(-1, -1);
			char dot = '.';
			int partSearchStartIndex = searchingStartIndex;


			// Split full name of property using dot. 
			//string[] commandParts = commandFullName.Split(new char[] { dot }, StringSplitOptions.RemoveEmptyEntries);

			int start = -1, end = -1;

			// locate property parts, only 2 parts!
			for (int i = 0; i < 2; i++)
			{
				int partIndex;
				string commandPart;
				string[] commandArrayUsed;
				if (i == 0)
					commandArrayUsed = commandFirstBase;
				else
					commandArrayUsed = commandSecondItem;

				//--------------------------------------------------
				// locate current part position with match case searching
				partIndex = Internal.FindCloserIndexOfItems(ref source, commandArrayUsed, partSearchStartIndex, out commandPart);

				// One part of property does not exists, so we should run away!
				if (partIndex == -1)
				{
					// if this is first part, is seems there isn't any more of this
					// else return last found position
					if (i == 0)
						result.Start = -1;
					else
						result.Start = partSearchStartIndex;

					// This negative result show's that the process failed
					result.End = -1;
					return result;
				}

				// saving start position
				if (i == 0)
					start = partIndex;

				//--------------------------------------------------
				// Test for free spaces after first part of name only
				if (i > 0)
				{
					// Test for free space between equal mark and semicolon (ex. window.location = 'text' )
					string shouldBeFreeSpace = source.Substring(partSearchStartIndex, partIndex - partSearchStartIndex);

					// Oops! this isn't our victim!
					if (shouldBeFreeSpace.Trim().Length > 0)
					{
						if (i == 0)
							result.Start = -1;
						else
							result.Start = partSearchStartIndex;

						result.End = -1;
						return result;
					}
				}

				// add part length to result
				result.Start = partIndex + commandPart.Length;

				// if only first part requested
				if (returnOnlyFirstPart == true)
				{
					// If only first part is needed so save it only if we are in first part
					if (i == 0)
						end = result.Start;
				}


				//--------------------------------------------------
				// If this isn't last part, we should locate next dot
				if (i == 0)
				{
					int dotPos;

					// Locate dot position after a part
					dotPos = Performance.IndexOfMatchCase(ref source, dot, result.Start);

					// There is not dot around here?!!
					if (dotPos == -1)
					{
						// result.Start = result.Start; // Same
						result.End = -1;
						return result;
					}

					// Test for free space between equal mark and semicolon (ex. window.location ('text'))
					string shouldBeFreeSpace = source.Substring(result.Start, dotPos - result.Start);

					// Oops! this isn't our victim!
					if (shouldBeFreeSpace.Trim().Length > 0)
					{
						// result.Start = result.Start;  // Same
						result.End = -1;
						return result;
					}

					// Set last position to dot position
					result.Start = dotPos + 1;
					partSearchStartIndex = result.Start;
				}

				// if only first part is not requested
				if (returnOnlyFirstPart == false)
					// every time result.Start is end of search result
					end = result.Start;
			}

			// if something found
			if (result.Start != -1)
			{
				int nextChar = Internal.FindNextValidCharacter(ref source, result.Start);
				if (nextChar != -1)
				{

					// Oops! this isn't our victim!
					if (source[nextChar] == shouldNotStartWithChar)
					{
						result.End = -1;
						return result;
					}
				}


				// Set found position to result
				result.Start = start;

				// The result is successfull 
				result.End = end;
			}

			return result;
		}

		private static int FindMethodHoleEndRange(ref string source, int methodStart, int serachEndRange)
		{
			int i = 0;
			char current = '\0';
			char previous = '\0';
			int countApostrophe = 0;
			int countQuote = 0;

			int countStartParenthesis = 0;
			int countEndParenthesis = 0;
			int countSemicolon = 0;
			int countVirgule = 0;
			int methodParameterEnd = -1;

			for (i = methodStart; i < source.Length; i++)
			{
				// If serching reached to end, break the loop.
				if (i > serachEndRange) break;

				// Set previous character
				previous = current;

				// Get current character
				current = source[i];

				switch (current)
				{
					case ('\''):// Apostrophe
						if (countQuote % 2 == 0)
						{
							if (previous == '\\')
							{
								if (!(countQuote % 2 != 0 || countApostrophe % 2 != 0))
									countApostrophe++;
							}
							else
								countApostrophe++;
						}
						break;


					case ('\"'): // Quote

						if (countApostrophe % 2 == 0)
						{
							if (previous == '\\')
							{
								if (!(countQuote % 2 != 0 || countApostrophe % 2 != 0))
									countQuote++;
							}
							else
								countQuote++;
						}
						break;



					case (','):// Virgule

						if ((countQuote % 2 == 0) && (countApostrophe % 2 == 0) && (countStartParenthesis == countEndParenthesis))
						{
							countVirgule++;
						}

						break;

					case ('('):// StartParenthesis

						if (!(countQuote % 2 != 0 || countApostrophe % 2 != 0))
							countStartParenthesis++;
						break;


					case (')'):// EndParenthesis

						if ((countQuote % 2 == 0) && (countApostrophe % 2 == 0) && (countEndParenthesis == countStartParenthesis))
						{
							methodParameterEnd = i;

							return methodParameterEnd;
						}
						else
							if (!(countQuote % 2 != 0 || countApostrophe % 2 != 0))
								countEndParenthesis++;
						break;


					case (';'):// Semicolon

						if ((countQuote % 2 == 0) && (countApostrophe % 2 == 0) && (countEndParenthesis - countStartParenthesis == 1))
						{
							// This is useless code but added to be sure about anything
							methodParameterEnd = i;

							// Return the position
							return methodParameterEnd;
						}
						else
							if (!(countQuote % 2 != 0 || countApostrophe % 2 != 0))
								countSemicolon++;
						break;
				}
			}

			return methodParameterEnd;
		}

		private static int FindMethodParameterEndRange(ref string source, int methodStart, int serachEndRange)
		{
			return FindMethodParameterEndRange(ref source, methodStart, serachEndRange, false);
		}
		private static int FindMethodParameterEndRange(ref string source, int methodStart, int serachEndRange, bool returnAllIfNoVirgule)
		{
			int i = 0;
			char current = '\0';
			char previous = '\0';
			int countApostrophe = 0;
			int countQuote = 0;

			int countStartParenthesis = 0;
			int countEndParenthesis = 0;
			int countSemicolon = 0;
			int methodParameterEnd = -1;

			for (i = methodStart; i < source.Length; i++)
			{
				// If serching reached to end, break the loop.
				if (i > serachEndRange) break;

				// Set previous character
				previous = current;

				// Get current character
				current = source[i];

				switch (current)
				{
					case ('\''):// Apostrophe
						if (countQuote % 2 == 0)
						{
							if (previous == '\\')
							{
								if (!(countQuote % 2 != 0 || countApostrophe % 2 != 0))
									countApostrophe++;
							}
							else
								countApostrophe++;
						}
						break;


					case ('\"'): // Quote

						if (countApostrophe % 2 == 0)
						{
							if (previous == '\\')
							{
								if (!(countQuote % 2 != 0 || countApostrophe % 2 != 0))
									countQuote++;
							}
							else
								countQuote++;
						}
						break;


					case (','):// Virgule

						if ((countQuote % 2 == 0) && (countApostrophe % 2 == 0) && (countStartParenthesis == countEndParenthesis))
						{
							methodParameterEnd = i;
							//break; // OOPs, this break exits from switch

							// Exit from For loop
							return methodParameterEnd;
						}

						break;


					case ('('):// StartParenthesis

						if (!(countQuote % 2 != 0 || countApostrophe % 2 != 0))
							countStartParenthesis++;
						break;

					case (')'):// EndParenthesis

						if ((countQuote % 2 == 0) && (countApostrophe % 2 == 0))
						{
							// Return method hole paremeters, if there is no more parameter
							if (returnAllIfNoVirgule && (countEndParenthesis == countStartParenthesis))
							{
								methodParameterEnd = i;
								return methodParameterEnd;
							}
							else
								if (countQuote % 2 == 0 && countApostrophe % 2 == 0)
									countEndParenthesis++;
						}
						break;

					case (';'):// Semicolon

						if (!(countQuote % 2 != 0 || countApostrophe % 2 != 0))
							countSemicolon++;
						break;
				}
			}

			return methodParameterEnd;
		}

		private static int FindPropertySetEndRange(ref string source, int propertyStart, int serachEndRange)
		{
			int i = 0;
			char current = '\0';
			char previous = '\0';
			int countApostrophe = 0;
			int countQuote = 0;

			int countStartParenthesis = 0;
			int countEndParenthesis = 0;
			//int countSemicolon = 0;
			int countVirgule = 0;
			int propertySetEnd = -1;
			int countRightBrace = 0;
			int countLeftBrace = 0;

			for (i = propertyStart; i < source.Length; i++)
			{
				// If serching reached to end, break the loop.
				if (i > serachEndRange) break;

				// Set previous character
				previous = current;

				// Get current character
				current = source[i];

				switch (current)
				{
					case ('\''):// Apostrophe
						if (countQuote % 2 == 0)
						{
							if (previous == '\\')
							{
								if (!(countQuote % 2 != 0 || countApostrophe % 2 != 0))
									countApostrophe++;
							}
							else
								countApostrophe++;
						}
						break;


					case ('\"'): // Quote

						if (countApostrophe % 2 == 0)
						{
							if (previous == '\\')
							{
								if (!(countQuote % 2 != 0 || countApostrophe % 2 != 0))
									countQuote++;
							}
							else
								countQuote++;
						}
						break;


					case (','):// Virgule

						if ((countQuote % 2 == 0) && (countApostrophe % 2 == 0) && (countStartParenthesis == countEndParenthesis))
						{
							countVirgule++;
						}

						break;


					case ('('):// StartParenthesis

						if (!(countQuote % 2 != 0 || countApostrophe % 2 != 0))
							countStartParenthesis++;
						break;


					case (')'):// EndParenthesis

						if (!(countQuote % 2 != 0 || countApostrophe % 2 != 0))
							countEndParenthesis++;
						break;


					case (';'):// Semicolon

						if (countQuote % 2 == 0 && countApostrophe % 2 == 0
							&& (countStartParenthesis == countEndParenthesis)

							// BUGFIX: v5 it forgotted brace characters!
							&& (countLeftBrace == countRightBrace))
						{
							propertySetEnd = i;

							return propertySetEnd;
							//countPint++; hehe
						}
						break;

					case ('{'):// Left brace

						if (countQuote % 2 == 0 && countApostrophe % 2 == 0)
							countLeftBrace++;
						break;

					case ('}'):// Right brace

						if ((countQuote % 2 == 0) && (countApostrophe % 2 == 0) && (countRightBrace == countLeftBrace))
						{
							// This is useless code but added to be sure about anything
							// this code will never run
							propertySetEnd = i;
							return propertySetEnd;
						}
						else
							if (countQuote % 2 == 0 && countApostrophe % 2 == 0)
								countRightBrace++;
						break;

				}
			}

			// If reached to end of script and scripter does not used (;) character
			// In properties, EndParenthesis and StartParenthesis should be equal
			if ((countQuote % 2 == 0) && (countApostrophe % 2 == 0) && (countEndParenthesis == countStartParenthesis))
			{
				// And, if we reached to end of script
				if (source.Length == i)
				{
					propertySetEnd = i;
				}
			}

			return propertySetEnd;
		}






		/// <summary>
		/// Useless. Just for test.
		/// </summary>
		private static string ReplaceChractersBetweenString(string source, int methodStart, char replaceCharacter, int serachEndRange)
		{
			int i = 0;
			char current = '\0';
			char previous = '\0';
			int countApostrophe = 0;
			int countQuote = 0;

			int countStartParenthesis = 0;
			int countEndParenthesis = 0;
			int countSemicolon = 0;
			StringBuilder result = new StringBuilder(source);
			int methodParameterEnd = 0;

			for (i = methodStart; i < source.Length; i++)
			{
				// If serching reached to end, break the loop.
				if (i > serachEndRange) break;

				// Set previous character
				previous = current;

				// Get current character
				current = source[i];

				switch (current)
				{
					case ('\''):// Apostrophe
						if (countQuote % 2 != 0)
						{
							result[i] = replaceCharacter;
							//countApostrophe = 0;
						}
						else
						{
							if (previous == '\\')
							{
								if (countQuote % 2 != 0 || countApostrophe % 2 != 0)
									result[i] = replaceCharacter;
								else
									countApostrophe++;
							}
							else
								countApostrophe++;
						}
						break;


					case ('\"'): // Quote

						if (countApostrophe % 2 != 0)
						{
							result[i] = replaceCharacter;
							//countQuote = 0;
						}
						else
						{
							if (previous == '\\')
							{
								if (countQuote % 2 != 0 || countApostrophe % 2 != 0)
									result[i] = replaceCharacter;
								else
									countQuote++;
							}
							else
								countQuote++;
						}
						break;


					case (','):// Virgule

						if ((countQuote % 2 == 0) && (countApostrophe % 2 == 0) && (countStartParenthesis == countEndParenthesis))
						{
							methodParameterEnd = i;
							//break; // OOPs this break exits from switch

							// Exit from For loop
							return result.ToString();
						}
						else
							result[i] = replaceCharacter;

						break;


					case ('('):// StartParenthesis

						if (countQuote % 2 != 0 || countApostrophe % 2 != 0)
						{
							result[i] = replaceCharacter;
						}
						else
							countStartParenthesis++;
						break;


					case (')'):// EndParenthesis

						if (countQuote % 2 != 0 || countApostrophe % 2 != 0)
						{
							result[i] = replaceCharacter;
						}
						else
							countEndParenthesis++;
						break;


					case (';'):// Semicolon

						if (countQuote % 2 != 0 || countApostrophe % 2 != 0)
						{
							result[i] = replaceCharacter;
						}
						else
							countSemicolon++;
						break;
				}
			}
			return result.ToString();
		}

		#endregion
	}
}