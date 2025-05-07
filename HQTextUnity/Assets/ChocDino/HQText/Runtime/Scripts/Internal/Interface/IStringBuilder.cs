//--------------------------------------------------------------------------//
// Copyright 2024-2024 Chocolate Dinosaur Ltd. All rights reserved.         //
// For full documentation visit https://www.chocolatedinosaur.com           //
//--------------------------------------------------------------------------//

namespace ChocDino.HQText.Internal
{
	/// <summary>
	/// An inteface for the string builder so we can change between different string builder
	/// implementations and decide which is fastest / best for a given usecase.
	/// </summary>
	public interface IStringBuilder
	{
		int GetLength();
		char[] GetArray();
		char Get(int index);
		void Set(int index, char ch);
		void SetValue(string text);
		void SetValue(IStringBuilder other);
		void Append(char ch);
		void Insert(int pos, char ch);
		void RemoveAll(char character);

		void Remove(int start, int length);

		void Replace(char oldChar, char newChar);

		void Replace(string oldStr, string newStr);

		void Replace(string oldStr, char newChar);

		void Clear();

		void Substring(IStringBuilder output, int start, int length);

		IStringBuilder[] Split(char[] splitOn);
	}
}
