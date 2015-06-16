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
using System.Runtime.Serialization.Json;
using System.Collections.Generic;

namespace ConsoleApplication1
{
    class Program
    {
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
            public void inputData() {
                past++;
                return;
            }
            public void refresh() {
                nowAction = NULL;
                past = 0;
                return;
            }
            public bool isStill() {
                return past >= still;
            }
            public bool isOver(){
                return past >= over;
            }
        }

        class Axis {
            private int num;
            private float stable;

            public Axis(int tNum, float tStable) {
                num = tNum;
                stable = tStable;
                return;
            }
            public void inputData(int tData) {
                num++;
                stable += (tData - stable)/num;
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
            static private bool indexClick;
            static private ActionTimer indexClickTimer;
            static private bool middleClick;
            static private ActionTimer middleClickTimer;
            static private bool middleRollDown;
            static private bool middleRollUp;
            static private bool indexRollDown;
            static private bool indexRollUp;
            static private bool thumbRollDown;
            static private bool thumbRollUp;

            static public void initiateFingerStatus() {
                indexClick = false;
                middleClick = false;
                middleRollDown = false;
                middleRollUp = false;
                indexRollDown = false;
                indexRollUp = false;
                thumbRollDown = false;
                thumbRollUp = false;
                // SETTINGS
                indexClickTimer = new ActionTimer(85, 190);
                middleClickTimer = new ActionTimer(85, 190);

                return;
            }

            private Axis x;
            private Axis y;
            private Axis z;
            private Data data;
            private String recordPath;
            private String inputPath;

            private void updateAxis() {
                x.inputData(data.x);
                y.inputData(data.y);
                z.inputData(data.z);
                return;
            }
            private void setAxisFromRecord(){
                if (recordPath == "") {
                    return;
                }
                StreamReader stream = new StreamReader(recordPath);
                String line = stream.ReadLine();
                String num = line.Convert.ToInt32(line);
                line = stream.ReadLine();
                String[] s = line.Split(",");
                x = new Axis(num, float.Parse(s[0], CultureInfo.InvariantCulture.NumberFormat));
                y = new Axis(num, float.Parse(s[1], CultureInfo.InvariantCulture.NumberFormat));
                z = new Axis(num, float.Parse(s[2], CultureInfo.InvariantCulture.NumberFormat));

                return;
            }
            private virtual void actionFilter();

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
                String inputPath = "";
                String line;
                while ((status == POWERON) && (line = inputPath.ReadLine) != null){
                    // ADDBLOCK: judge if break the process

                    char[] splitChar = {"\t"};
                    String[] s = line.Split(splitChar);

                    // ADDBLOCK: judge if the input line is complete
                    // however, this might use no less than 3 input lines

                    setData(Convert.ToInt32(s[0]), Convert.ToInt32(s[1]), Convert.ToInt32(s[2]));
                    actionFilter();
                }
                return;
            }
        }

        class Thumb : Finger {
            private bool powerDown(){
                // ADDBLOCK: If power Down is triggered
                System.WriteLine("power down is triggered!");
                return false;
            }
            private bool cursorLeftRight() {
                // ADDBLOCK: action-cursor move left-right triggered \
                // using axis-y
                if ((data.y - y.stable) >= THRESHOLD.MOVECURSORUP) {
                    // 
                } else {
                    // 
                }
                System.WriteLine("CURSORMOVE LEFT RIGHT triggered!");
                return false;
            }
            private bool isPowerDown() {
                // ADDBLOCK: Conditions that make power down
                return false;
            }
            private bool isMove() {
                // Check if the cursor MOVING UP or DOWN \
                // is triggered (using axis-y)
                return ((data.y - y.stable) >= THRESHOLD.MOVECURSORUP) 
                    || ((y.stable - data.y) >= THRESHOLD.MOVECURSORDOWN);
            }
            private override void actionFilter() {
                // Check if the action is lasting \
                // or in the still time
                switch (nowAction) {
                    // case CURSORMOVE:    // ADDBLOCK: If cursor moving goes on \
                    //     // listen to the values of index \
                    //     // for cursor move up or down
                    // 
                    //     System.WriteLine("cursor move go on");
                    // break;
                    case NULL:
                        if (isPowerDown()) {
                            powerDown();
                        } else if (isMove()) {
                            cursorLeftRight();
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
                // ADDBLOCK: left click
                System.WriteLine("LEFTCLICK triggered!");
                return false;
            }
            private bool cursorUpDown() {
                // ADDBLOCK: action-cursor move up-down triggered \
                // using axis-y
                if ((data.y - y.stable) >= THRESHOLD.MOVECURSORUP) {
                    // 
                } else {
                    // 
                }
                System.WriteLine("CURSORMOVE UPDOWN triggered!");
                return false;
            }
            private bool isClick() {
                return (data.z - z.stable) >= THRESHOLD.LEFTCLICK;
            }
            private bool isMove() {
                // Check if the cursor MOVING UP or DOWN \
                // is triggered (using axis-y)
                return ((data.y - y.stable) >= THRESHOLD.MOVECURSORUP) 
                    || ((y.stable - data.y) >= THRESHOLD.MOVECURSORDOWN);
            }
            private override void actionFilter() {
                // Check if the action is lasting \
                // or in the still time
                switch (nowAction) {
                    case LEFTCLICK:     // If it was left_click \
                        // go on still time \ 
                        // and check if it is over
                        indexClickTimer.inputData();
                        if (!indexClickTimer.isStill() && !indexClickTimer.isOver()){
                            isClick();
                        } else if (indexClickTimer.isOver()) {
                            indexClickTimer.refresh();
                        }
                    break;
                    case ROLL:          // If rolling, follow the middle finger action
                    break;
                    // case CURSORMOVE:    // ADDBLOCK: If cursor moving goes on \
                    //     // listen to the values of index \
                    //     // for cursor move up or down
                    // 
                    //     System.WriteLine("cursor move go on");
                    // break;
                    case NULL:
                        if (isClick()) {
                            indexClickTimer.refresh();
                            nowAction = LEFTCLICK;
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
                // ADDBLOCK: right click
                System.WriteLine("right click triggered!");
                return false;
            }
            private bool roll() {
                // ADDBLOCK: action-cursor move up-down triggered \
                // using axis-y
                if ((data.y - y.stable) >= THRESHOLD.MOVECURSORUP) {
                    // 
                } else {
                    // 
                }
                System.WriteLine("ROLL UPDOWN triggered!");
                return false;
            }
            private bool isClick() {
                return (data.z - z.stable) >= THRESHOLD.RIGHTCLICK;
            }
            private bool isRoll() {
                return false;
            }
            private override void actionFilter() {
                // Check if the action is lasting \
                // or in the still time
                switch (nowAction) {
                    case LEFTCLICK:     // If it was right_click \
                        // go on still time \ 
                        // and check if it is over
                        middleClickTimer.inputData();
                        if (!middleClickTimer.isStill() && !middleClickTimer.isOver()){
                            isClick();
                        } else if (middleClickTimer.isOver()) {
                            middleClickTimer.refresh();
                        }
                    break;
                    case NULL:
                        if (isClick()) {
                            System.WriteLine("middle click!");
                        } else if (isRoll()) {
                            System.WriteLine("middle roll!");
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

        enum ACTION {NULL, POWERON, POWEROFF, CURSORMOVE, RIGHTCLICK, LEFTCLICK, ROLL};
        class THRESHOLD {
            static public float LEFTCLICK;
            static public float RIGHTCLICK;
            static public float ROLLUP;
            static public float ROLLDOWN;
            static public float MOVECURSORUP;
            static public float MOVECURSORDOWN;
        }

        static private int THRESHOLD;
        static private ACTION status;
        static private ACTION nowAction;

        static private void initiate() {
            // SETTINGS
            status = POWERON;
            nowAction = NULL;
            THRESHOLD.LEFTCLICK = 8000;
            THRESHOLD.RIGHTCLICK = 8000;
            THRESHOLD.ROLLUP = 500;
            THRESHOLD.ROLLDOWN = 500;
            THRESHOLD.MOVECURSORDOWN = 500;
            THRESHOLD.MOVECURSORUP = 500;
            Finger.initiateFingerStatus();
            return;
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