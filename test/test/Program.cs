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
            public void refresh() {
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
            private virtual void isAction();

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
                while (STATUS && (line = inputPath.ReadLine) != null){
                    // ADDBLOCK: judge if break the process

                    char[] splitChar = {"\t"};
                    String[] s = line.Split(splitChar);

                    // ADDBLOCK: judge if the input line is complete
                    // however, this might use no less than 3 input lines

                    setData(Convert.ToInt32(s[0]), Convert.ToInt32(s[1]), Convert.ToInt32(s[2]));
                    isAction();
                }
                return;
            }
        }

        class Thumb : Finger {
            // recognize functions
            private bool isPowerTrigger() {
                // ADDBLOCK
                return false;
            }
            private bool isMove() {
                // ADDBLOCK
                return false;
            }
            private override void isAction() {
                if (isPowerTrigger()) {
                    System.WriteLine("thumb trigger!");
                } else if (isMove()) {
                    System.WriteLine("thumb move!");
                }
                return;
            }

            // creation function
            public Thumb() {
                data = new Data();
                recordPath = "../../../records/thumb.record";
                setAxisFromRecord();
                inputPath = "../../../data/6/thumb.data";
                return;
            }
        }

        class Index : Finger {
            private bool isClick() {
                return false;
            }
            private bool isMove() {
                return false;
            }
            private override void isAction() {
                if (isClick()) {
                    System.WriteLine("index click!");
                } else if (isMove()) {
                    System.WriteLine("index move!");
                }
                return;
            }

            public Index() {
                data = new Data();
                recordPath = "../../../records/index.record";
                setAxisFromRecord();
                inputPath = "../../../data/6/index.data";
                return;
            }
        }

        class Middle : Finger {
            private bool isClick() {
                return false;
            }
            private bool isRoll() {
                return false;
            }
            private override void isAction() {
                if (isClick()) {
                    System.WriteLine("middle click!");
                } else if (isRoll()) {
                    System.WriteLine("middle roll!");
                }
                return;
            }

            public Middle() {
                data = new Data();
                recordPath = "../../../records/middle.record";
                setAxisFromRecord();
                inputPath = "../../../data/6/middle.data";
                return;
            }
        }

        static private bool STATUS; // true for working and false for pause

        static private void initiate() {
            STATUS = true;
            Finger.initiateFingerStatus();
            return;
        }

        static void Main(String[] args) {
            initiate();

            Thumb thumb = new Thumb();
            Index index = new Index();
            Middle middle = new Middle();

            thumb.startAction();

            Console.ReadKey();
            return ;
        }
    }
}
