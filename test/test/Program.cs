/*
*   author:     tristan
*   email:      tristan.xiery@gmail.com
*   tips:
*               ADDBLOCK    where should add function code block
*               SETTINGS    where the settings are set static
*   optimize:   
*               1. the roll down function impact the average line
*               2. if the index-click impact the data of middle \
*                   , I can choose the one that is more obvious
*               3. set the comparision of prior and later value \
*                   to avoid click-roll-move confusion
*               4. temporarily, the moving cursor and roll \
*                   are processed in judging functions
*/ 

using System;
using System.ComponentModel;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Text;
using System.Security;
using System.Security.Permissions;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.Data;

namespace ConsoleApplication1 {
    class Program {
        // Write consoles into a file
        // public static String consoleFolderName = @"../../../consoles";
        // public static String timeStamp = DateTime.Now.ToString();
        // public static String consoleFileName =  System.IO.Path.Combine(consoleFolderName, "2");
        // // public System.IO.FileStream createdFile = System.IO.File.Create(consoleFileName);
        // public static System.IO.StreamWriter consoleFile = 
        //     new System.IO.StreamWriter(consoleFileName, true);
        // public Cursor nCursor = new Cursor(Cursor.Current.Handle);

        // Mouse event settings
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern int mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);
        const int MOUSEEVENTF_MOVE = 0x0001;
        const int MOUSEEVENTF_LEFTDOWN = 0x0002;
        const int MOUSEEVENTF_LEFTUP = 0x0004;
        const int MOUSEEVENTF_RIGHTDOWN = 0x0008;
        const int MOUSEEVENTF_RIGHTUP = 0x0010;
        const int MOUSEEVENTF_VWHEEL = 0x0800;
        // mouse_event(0x0800, 0, 0, 500, 0);
        // const int MOUSEEVENTF_MIDDLEDOWN = 0x0020;
        // const int MOUSEEVENTF_MIDDLEUP = 0x0040;
        // const int MOUSEEVENTF_ABSOLUTE = 0x8000;
        // const int MOUSEEVENTF_HWHEEL = 0x1000;
        // mouse_event(0x1000, 0, 0, 500, 0);


        enum ACTION {NULL, POWERON, POWEROFF, CURSORMOVE, RIGHTCLICK, LEFTCLICK, ROLL};
        class THRESHOLD {
            static public float LEFTCLICK;
            static public float RIGHTCLICK;
            static public float ROLLUP;
            static public float ROLLDOWN;
            static public float MOVECURSORUP;
            static public float MOVECURSORDOWN;
        }

        static private ACTION status;
        static private ACTION nowAction;

        static private void initiate() {
            // SETTINGS
            status = ACTION.POWERON;
            nowAction = ACTION.NULL;
            THRESHOLD.LEFTCLICK = 9000;
            THRESHOLD.RIGHTCLICK = 8000;
            THRESHOLD.ROLLUP = 500;
            THRESHOLD.ROLLDOWN = 500;
            THRESHOLD.MOVECURSORUP = 1500;
            THRESHOLD.MOVECURSORDOWN = 1500;
            Finger.initiateFingerStatus();

            return;
        }

        class Data {
            public int x;
            public int y;
            public int z;
        }

        class ActionTimer {
            private int past;
            private int still;
            private int over;

            public ActionTimer(int tStill, int tOver) {
                still = tStill;
                over = tOver;
                past = 0;
                return;
            }
            public ActionTimer(int tStill) {
                over = still = tStill;
                past = 0;
                return;
            }
            public void inputData() {
                past++;
                return;
            }
            public void refresh() {
                nowAction = ACTION.NULL;
                past = 0;
                return;
            }
            public bool isStill() {
                return past <= still;
            }
            public bool isOver(){
                return past >= over;
            }

            public int getPast() {
                return past;
            }
        }

        class Axis {
            private int num;
            private float stable;

            // temp number
            public int tempNum = 0;

            public Axis(int tNum, float tStable) {
                num = tNum;
                stable = tStable;
                return;
            }
            public void inputData(int tData) {
                num++;
                stable += (tData - stable)/num;

                tempNum++;
                return;
            }
            public int getNum() {
                return num;
            }
            public float getStable() {
                return stable;
            }
        }

        class Finger {
            static protected ActionTimer thumbCursorTimer;
            static protected ActionTimer indexClickTimer;
            static protected ActionTimer middleClickTimer;
            static protected ActionTimer middleRollTimer;

            static public void initiateFingerStatus() {
                // SETTINGS
                // thumbOnOffTimer = new ActionTimer(3);
                thumbCursorTimer = new ActionTimer(3);
                // indexCursorTimer = new ActionTimer(3);
                middleRollTimer = new ActionTimer(10);
                indexClickTimer = new ActionTimer(105, 190);
                middleClickTimer = new ActionTimer(105, 190);

                return;
            }

            protected String recordPath;
            protected String inputPath;
            protected Data data;
            protected Axis x;
            protected Axis y;
            protected Axis z;

            protected void updateAxis() {
                x.inputData(data.x);
                y.inputData(data.y);
                z.inputData(data.z);
                return;
            }
            protected void setAxisFromRecord(){
                if (recordPath == "") {
                    return;
                }
                StreamReader stream = new StreamReader(recordPath);
                String line = stream.ReadLine();
                int num = Convert.ToInt32(line);
                line = stream.ReadLine();
                char[] splitChar = {','};
                String[] s = line.Split(splitChar);
                x = new Axis(num, float.Parse(s[0]));
                y = new Axis(num, float.Parse(s[1]));
                z = new Axis(num, float.Parse(s[2]));

                return;
            }
            protected virtual void actionFilter() {}

            public Finger() {}
            public Data getData(){
                return data;
            }
            public void setData(int tX, int tY, int tZ) {
                data.x = tX;
                data.y = tY;
                data.z = tZ;
                updateAxis();
                return;
            }
            public void startAction(){
                String line;
                StreamReader stream = new StreamReader(inputPath);
                while ((status == ACTION.POWERON) && (line = stream.ReadLine()) != null){
                    char[] splitChar = {'\t'};
                    String[] s = line.Split(splitChar);

                    setData(Convert.ToInt32(s[0]), Convert.ToInt32(s[1]), Convert.ToInt32(s[2]));
                    actionFilter();
                }

                return;
            }
        }

        class Thumb : Finger {
            private bool powerDown(){
                // ADDBLOCK: Make power down
                Console.WriteLine("power down is triggered! ----- " + x.tempNum.ToString());
                return false;
            }
            private bool cursorLeftRight() {
                // Cursor movements when left-right triggered \
                // using axis-y
                int more = 0;
                int less = 0;
                int cursorX = 0;

                if ((more = (data.y - (int)(y.getStable()))) >= THRESHOLD.MOVECURSORUP) {
                    if (more <= 2000) {
                        // speed 1
                        cursorX = 1;
                    } else if (more <= 2500) {
                        // speed 2
                        cursorX = 2;
                    } else if (more <= 3000) {
                        // speed 3
                        cursorX = 3;
                    } else {
                        // speed 4
                        cursorX = 4;
                    }
                } else {
                    less = (int)y.getStable() - data.y;
                    if (less <= 2000) {
                        // speed 1
                        cursorX = -1;
                    } else if (less <= 2500) {
                        // speed 2
                        cursorX = -2;
                    } else if (less <= 3000) {
                        // speed 3
                        cursorX = -3;
                    } else {
                        // speed 4
                        cursorX = -4;
                    }
                }

                // System.Threading.Thread.Sleep(500);
                // mouse_event(MOUSEEVENTF_MOVE, Cursor.Position.X, Cursor.Position.Y, 0, 0);
                return false;
            }
            private bool isPowerDown() {
                // ADDBLOCK: Conditions that make power down
                return false;
            }
            private bool isMoveCursor() {
                // Check if the cursor MOVING UP or DOWN \
                // is triggered (using axis-y)
                return ((data.y - y.getStable()) >= THRESHOLD.MOVECURSORUP) 
                    || ((y.getStable() - data.y) >= THRESHOLD.MOVECURSORDOWN);
            }
            protected override void actionFilter() {
                // Check if the action is lasting \
                // or in the still time
                switch (nowAction) {
                    case ACTION.CURSORMOVE:    // If cursor moving goes on \
                        // listen to the values of index \
                        // for cursor move up or down
                        if (!isMoveCursor() && !thumbCursorTimer.isStill()) {
                            nowAction = ACTION.NULL;
                            cursorLeftRight();
                        }
                        thumbCursorTimer.inputData();
                        if (thumbCursorTimer.getPast() == 3) {
                            cursorLeftRight();
                        }
                    break;
                    case ACTION.NULL:
                        if (isPowerDown()) {
                            powerDown();
                        } else if (isMoveCursor()) {
                            nowAction = ACTION.CURSORMOVE;
                        }
                    break;
                    default:
                    break;
                }
                
                return;
            }

            public Thumb() {
                // SETTINGS
                data = new Data();
                recordPath = "../../../records/thumb.record";
                setAxisFromRecord();
                inputPath = "../../../data/6/thumb.data";
                return;
            }
        }

        class Index : Finger {
            private bool leftClick() {
                mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
                mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
                return false;
            }
            private bool cursorUpDown() {
                // Cursor move up-down triggered \
                // using axis-y
                int more = 0;
                int less = 0;
                int cursorY = 0;

                if ((more = (data.y - (int)(y.getStable()))) >= THRESHOLD.MOVECURSORUP) {
                    if (more <= 2000) {
                        // speed 1
                        cursorY = 1;
                    } else if (more <= 2500) {
                        // speed 2
                        cursorY = 2;
                    } else if (more <= 3000) {
                        // speed 3
                        cursorY = 3;
                    } else {
                        // speed 4
                        cursorY = 4;
                    }
                } else {
                    less = (int)y.getStable() - data.y;
                    if (less <= 2000) {
                        // speed 1
                        cursorY = -1;
                    } else if (less <= 2500) {
                        // speed 2
                        cursorY = -2;
                    } else if (less <= 3000) {
                        // speed 3
                        cursorY = -3;
                    } else {
                        // speed 4
                        cursorY = -4;
                    }
                }

                // System.Threading.Thread.Sleep(1000);
                // mouse_event(MOUSEEVENTF_MOVE, Cursor.Position.X, Cursor.Position.Y + cursorY, 0, 0);
                return false;
            }
            private bool isClick() {
                return (data.z - z.getStable()) >= THRESHOLD.LEFTCLICK;
            }
            private bool isMove() {
                // Check if the cursor MOVING UP or DOWN \
                // is triggered (using axis-y)
                return ((data.y - y.getStable()) >= THRESHOLD.MOVECURSORUP) 
                    || ((y.getStable() - data.y) >= THRESHOLD.MOVECURSORDOWN);
            }
            protected override void actionFilter() {
                // Check if the action is lasting \
                // or in the still time
                switch (nowAction) {
                    case ACTION.LEFTCLICK:     // If it was left_click \
                        // go on still time \ 
                        // and check if it is over
                        indexClickTimer.inputData();
                        if (!indexClickTimer.isStill() && !indexClickTimer.isOver() && isClick()){
                            leftClick();
                        } else if (indexClickTimer.isOver()) {
                            indexClickTimer.refresh();
                        }
                    break;
                    case ACTION.ROLL:          // If rolling, follow the middle finger action
                    break;
                    // case ACTION.CURSORMOVE:    // ADDBLOCK: If cursor moving goes on \
                    //     // listen to the values of index \
                    //     // for cursor move up or down
                    
                    //     Console.WriteLine("cursor move go on");
                    // break;<<<<<
                    case ACTION.NULL:
                        if (isClick()) {
                            indexClickTimer.refresh();
                            nowAction = ACTION.LEFTCLICK;
                            leftClick();
                        } else if (isMove()) {
                            cursorUpDown();
                        }
                    break;
                    default:
                    break;
                }
                
                return;
            }

            public Index() {
                // SETTINGS
                data = new Data();
                recordPath = "../../../records/index.record";
                setAxisFromRecord();
                inputPath = "../../../data/6/index.data";
                return;
            }
        }

        class Middle : Finger {
            private bool rightClick() {
                mouse_event(MOUSEEVENTF_RIGHTDOWN, 0, 0, 0, 0);
                mouse_event(MOUSEEVENTF_RIGHTUP, 0, 0, 0, 0);
                return false;
            }
            private bool roll() {
                // Cursor move up-down triggered \
                // using axis-y
                int more = 0;
                int less = 0;
                int scrollY = 0;

                if ((more = (data.z - (int)(z.getStable()))) >= THRESHOLD.MOVECURSORUP) {
                    if (more <= 2000) {
                        // speed 1
                        scrollY = 1;
                    } else if (more <= 2500) {
                        // speed 2
                        scrollY = 2;
                    } else if (more <= 3000) {
                        // speed 3
                        scrollY = 3;
                    } else {
                        // speed 4
                        scrollY = 4;
                    }
                } else {
                    less = (int)y.getStable() - data.y;
                    if (less <= 2000) {
                        // speed 1
                        scrollY = 1;
                    } else if (less <= 2500) {
                        // speed 2
                        scrollY = 2;
                    } else if (less <= 3000) {
                        // speed 3
                        scrollY = 3;
                    } else {
                        // speed 4
                        scrollY = 4;
                    }
                }
                Console.WriteLine("MOUSEEVENTF_VWHEEL --- {0}", scrollY);
                // mouse_event(MOUSEEVENTF_VWHEEL, 0, 0, scrollY, 0);
                // System.Threading.Thread.Sleep(500);
                return false;
            }
            private bool isClick() {
                return (data.z - z.getStable()) >= THRESHOLD.RIGHTCLICK;
            }
            private bool isRoll() {
                return ((data.z - z.getStable()) >= THRESHOLD.ROLLUP) 
                    || ((z.getStable() - data.z) >= THRESHOLD.ROLLDOWN);
            }
            protected override void actionFilter() {
                // Check if the action is lasting \
                // or in the still time
                switch (nowAction) {
                    case ACTION.RIGHTCLICK:     // If it was right_click \
                        // go on still time \ 
                        // and check if it is over
                        middleClickTimer.inputData();
                        if (!middleClickTimer.isStill() && !middleClickTimer.isOver() && isClick()){
                            rightClick();
                        } else if (middleClickTimer.isOver()) {
                            middleClickTimer.refresh();
                        }
                    break;
                    case ACTION.ROLL:   // If it was a vertical scroll \
                        // go on still time \ 
                        // and check if it is over
                        middleRollTimer.inputData();
                        if (!middleRollTimer.isStill() && !middleRollTimer.isOver() && isRoll()){
                            roll();
                        } else if (middleRollTimer.isOver()) {
                            middleRollTimer.refresh();
                        }
                    break;
                    case ACTION.NULL:
                        if (isClick()) {
                            middleClickTimer.refresh();
                            nowAction = ACTION.RIGHTCLICK;
                            rightClick();
                        } else if (isRoll()) {
                            middleRollTimer.refresh();
                            nowAction = ACTION.ROLL;
                            roll();
                        }
                    break;
                    default:
                    break;
                }

                return;
            }

            public Middle() {
                // SETTINGS
                data = new Data();
                recordPath = "../../../records/middle.record";
                setAxisFromRecord();
                inputPath = "../../../data/6/middle.data";
                return;
            }
        }


        static void Main(String[] args) {
            initiate();

            Thumb thumb = new Thumb();
            Index index = new Index();
            Middle middle = new Middle();

            thumb.startAction();
            index.startAction();
            middle.startAction();

            Console.ReadKey();
            return ;
        }
    }
}