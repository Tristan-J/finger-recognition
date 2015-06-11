/*
* name: CodeFile1.cs
* description: Main fuction of recognition system
* author: tristan
* date & version: Thu May 14 16:52:28	 2015
*/

using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace recognitionTest2 {

	class Program {
		static void Main(string[] args) {

			// recognitize actions and change status from thumb/index/middle data
			runRecognition();

			Console.ReadKey();
			return;
		}

		static private int UP = 0;
		static private int DOWN = 1;
		static private int 	OTHER = 2;

		static private int[] thumbData = new int[3];
		static private int[] indexData = new int[3];
		static private int[] middleData = new int[3];

		static private void thumbAction(int dataAction, int inputData, int stableData) {
			if (dataAction == UP) {
				cursorToLeft(inputData, stableData);
			} else {
				cursorToRight(inputData, stableData);
			}

			return;
		}

		static private void indexAction(int dataAction, int inputData, int stableData) {
			if (dataAction == UP) {
				cursorToUp(inputData, stableData);
			} else if (dataAction == DOWN) {
				cursorToDown(inputData, stableData);
			} else {
				leftClick();
			}

			return;
		}

		static private void middleAction(int dataAction, int inputData, int stableData) {
			if (dataAction == UP) {
				rollUp(inputData, stableData);
			} else if (dataAction == DOWN) {
				rollDown(inputData, stableData);
			} else {
				rightClick();
			}
		}

		private static void runRecognition() {
			return;
		}

		// -------------------- control mouse --------------------
		static private void cursorToLeft(int inputData, int stableData) {
			return;
		}

		static private void cursorToRight(int inputData, int stableData) {
			return;
		}

		static private void cursorToUp(int inputData, int stableData) {
			return;
		}

		static private void cursorToDown(int inputData, int stableData) {
			return;
		}

		static private void rollUp(int inputData, int stableData) {
			return;
		}

		static private void rollDown(int inputData, int stableData) {
			return;
		}

		static private void leftClick() {
			return;
		}

		static private void rightClick() {
			return;
		}

	}
}