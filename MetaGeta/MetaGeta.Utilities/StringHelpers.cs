// Copyright 2009 Will Thomas
// 
// This file is part of MetaGeta.
// 
// MetaGeta is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// MetaGeta is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MetaGeta. If not, see <http://www.gnu.org/licenses/>.

#region

using System;
using System.Runtime.InteropServices;
using System.Text;

#endregion

namespace MetaGeta.Utilities {
    public class StringHelpers {
        public static string MultiplyString(string s, int count) {
            var sb = new StringBuilder();

            for (int index = 0; index <= count; index++)
                sb.Append(s);

            return sb.ToString();
        }

        public static void DrawProgressBar(int progress, int total) {
            int width = 60;

            Console.CursorLeft = 0;
            Console.Write("[");
            Console.CursorLeft = width + 2;
            Console.Write("]");
            Console.CursorLeft = 1;
            int onechunk = width / total;

            int position = 1;

            for (int i = 0; i <= onechunk * progress; i++) {
                Console.BackgroundColor = ConsoleColor.Green;
                Console.CursorLeft = position;
                position += 1;
                Console.Write(" ");
            }

            for (int i = position; i <= width + 1; i++) {
                Console.BackgroundColor = ConsoleColor.Black;
                Console.CursorLeft = position;
                position += 1;
                Console.Write(" ");
            }


            Console.CursorLeft = width + 5;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.Write(progress + " of " + total + "    ");
        }

        public static void DrawProgressBar(double progress, string extraText) {
            int width = 60;

            Console.CursorTop -= 1;
            Console.CursorLeft = 0;
            Console.Write(extraText);

            Console.CursorTop += 1;

            Console.CursorLeft = 0;
            Console.Write("[");
            Console.CursorLeft = width + 2;
            Console.Write("]");
            Console.CursorLeft = 1;

            int position = 1;

            for (int i = 0; i <= width * progress; i++) {
                Console.BackgroundColor = ConsoleColor.Green;
                Console.CursorLeft = position;
                position += 1;
                Console.Write(" ");
            }

            for (int i = position; i <= width + 1; i++) {
                Console.BackgroundColor = ConsoleColor.Gray;
                Console.CursorLeft = position;
                position += 1;
                Console.Write(" ");
            }


            Console.CursorLeft = width + 5;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.Write(progress.ToString("#0%"));
        }
    }
}