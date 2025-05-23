//--------------------------------------------------------------------------//
// Copyright 2024-2024 Chocolate Dinosaur Ltd. All rights reserved.         //
// For full documentation visit https://www.chocolatedinosaur.com           //
//--------------------------------------------------------------------------//

using System;
using System.Collections.Generic;

namespace ChocDino.HQText.Internal
{
	// Original Author: Mohamad Narimani. Used under MIT License.
	public class FastStringBuilder : IStringBuilder {
		// Using fields to be as efficient as possible
		public int Length;

		private char[] array;
		private int capacity;

		public FastStringBuilder() {
		this.capacity = 2048;
		array = new char[capacity];
		}

		public FastStringBuilder(int capacity) {
		if (capacity < 0)
			throw new ArgumentOutOfRangeException(nameof(capacity));

		this.capacity = capacity;
		array = new char[capacity];
		}

		public FastStringBuilder(string text) : this(text, text.Length) {}

		public FastStringBuilder(string text, int capacity) : this(capacity) { SetValue(text); }

		public char Get(int index) { return array[index]; }

		public void Set(int index, char ch) { array[index] = ch; }

		public void SetValue(string text) {
		Length = text.Length;
		EnsureCapacity(Length, false);

		for (int i = 0; i < text.Length; i++)
			array[i] = text[i];
		}

		public void SetValue(IStringBuilder other) {
		EnsureCapacity(other.GetLength(), false);
		Copy(other.GetArray(), array);
		Length = other.GetLength();
		}

		public void Append(char ch) {
		if (capacity <= Length)
			EnsureCapacity(Length, true);

		array[Length] = ch;
		Length++;
		}

		public void Insert(int pos, IStringBuilder str, int offset, int count) {
		if (str == this)
			throw new InvalidOperationException("You cannot pass the same string builder to insert");
		if (count == 0)
			return;

		Length += count;
		EnsureCapacity(Length, true);

		for (int i = Length - count - 1; i >= pos; i--) {
			array[i + count] = array[i];
		}
		char[] otherArray = str.GetArray();
		for (int i = 0; i < count; i++) {
			array[pos + i] = otherArray[offset + i];
		}
		}

		public void Insert(int pos, IStringBuilder str) { Insert(pos, str, 0, str.GetLength()); }

		public void Insert(int pos, char ch) {
		Length++;
		EnsureCapacity(Length, true);

		for (int i = Length - 2; i >= pos; i--)
			array[i + 1] = array[i];

		array[pos] = ch;
		}

		public void RemoveAll(char character) {
		for (int i = Length - 1; i >= 0; i--) {
			if (array[i] == character)
			Remove(i, 1);
		}
		}

		public void Remove(int start, int length) {
		for (int i = start; i < Length - length; i++) {
			array[i] = array[i + length];
		}

		Length -= length;
		}

		public void Reverse(int startIndex, int length) {
		for (int i = 0; i < length / 2; i++) {
			int firstIndex = startIndex + i;
			int secondIndex = startIndex + length - i - 1;

			char first = array[firstIndex];
			char second = array[secondIndex];

			array[firstIndex] = second;
			array[secondIndex] = first;
		}
		}

		public void Reverse() { Reverse(0, Length); }

		public void Substring(IStringBuilder output, int start, int length) {
		output.Clear();
		for (int i = 0; i < length; i++)
			output.Append(array[start + i]);
		}

		public override string ToString() { return new string(array, 0, Length); }

		public void Replace(char oldChar, char newChar) {
		for (int i = 0; i < Length; i++) {
			if (array[i] == oldChar)
			array[i] = newChar;
		}
		}

		// I got ChatGPT to help me rewrite this - Shane Marks.
		public void Replace(string oldStr, char newStr) {
		if (oldStr.Length < 1) {
			return;
		}

		int writeIdx = 0;
		for (int readIdx = 0; readIdx < Length; readIdx++) {
			bool match = true;
			for (int j = 0; j < oldStr.Length; j++) {
			if (readIdx + j >= Length || array[readIdx + j] != oldStr[j]) {
				match = false;
				break;
			}
			}

			if (match) {
			array[writeIdx++] = newStr;
			readIdx += oldStr.Length - 1;
			} else {
			array[writeIdx++] = array[readIdx];
			}
		}

		Length = writeIdx;
		}

		public void Replace(string oldStr, string newStr) {
		for (int i = 0; i < Length; i++) {
			bool match = true;
			for (int j = 0; j < oldStr.Length; j++) {
			if (array[i + j] != oldStr[j]) {
				match = false;
				break;
			}
			}

			if (!match)
			continue;

			if (oldStr.Length == newStr.Length) {
			for (int k = 0; k < oldStr.Length; k++) {
				array[i + k] = newStr[k];
			}
			} else if (oldStr.Length < newStr.Length) {
			// We need to expand capacity
			int diff = newStr.Length - oldStr.Length;
			Length += diff;
			EnsureCapacity(Length, true);

			// Move everything forward by difference of length
			for (int k = Length - diff - 1; k >= i + oldStr.Length; k--) {
				array[k + diff] = array[k];
			}

			// Start writing new string
			for (int k = 0; k < newStr.Length; k++) {
				array[i + k] = newStr[k];
			}
			} else {
			// We need to shrink
			int diff = oldStr.Length - newStr.Length;

			// Move everything backwards by diff
			for (int k = i + diff; k < Length - diff; k++) {
				array[k] = array[k + diff];
			}

			for (int k = 0; k < newStr.Length; k++) {
				array[i + k] = newStr[k];
			}

			Length -= diff;
			}

			i += newStr.Length;
		}
		}

		public void Clear() { Length = 0; }

		private void EnsureCapacity(int cap, bool keepValues) {
		if (capacity > cap)
			return;

		if (capacity == 0)
			capacity = 1;

		while (capacity <= cap)
			capacity *= 2;

		if (keepValues) {
			char[] newArray = new char[capacity];
			Copy(array, newArray);
			array = newArray;
		} else {
			array = new char[capacity];
		}
		}

		private static void Copy(char[] src, char[] dst) {
		for (int i = 0; i < src.Length; i++)
			dst[i] = src[i];
		}

		public int GetLength() { return Length; }

		public char[] GetArray() { return array; }

		public List<IStringBuilder> Split(char[] splitOn) {
		List<IStringBuilder> sections = new List<IStringBuilder>();
		IStringBuilder section = new FastStringBuilder(0);
		for (int i = 0; i < Length; i++) {
			bool match = false;
			for (int c = 0; c < splitOn.Length; c++) {
			if (array[i] == splitOn[c]) {
				match = true;
				break;
			}
			}

			if (!match) {
			section.Append(array[i]);
			} else {
			sections.Add(section);
			section = new FastStringBuilder(0);
			}
		}
		// asppe
		if (section.GetLength() > 0) {
			sections.Add(section);
		}
		return sections;
		}

		IStringBuilder[] IStringBuilder.Split(char[] splitOn) {
		string[] elements = ToString().Split(splitOn);

		IStringBuilder[] sections = new IStringBuilder[elements.Length];

		for (int i = 0; i < elements.Length; i++) {
			sections[i] = new FastStringBuilder(elements[i]);
		}
		return sections;
		}
	}
}
