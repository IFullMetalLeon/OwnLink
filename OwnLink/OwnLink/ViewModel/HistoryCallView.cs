﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using OwnLink.Model;

namespace OwnLink.ViewModel
{
    public class HistoryCallView: INotifyPropertyChanged
    {
        public HistoryCall HistoryCall { get; set; }
        public HistoryCallView()
        {
            HistoryCall = new HistoryCall();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }

        public string Id
        {
            get
            {
                return HistoryCall.id;
            }
            set
            {
                if (HistoryCall.id != value)
                {
                    HistoryCall.id = value;
                    OnPropertyChanged("Id");
                }
            }
        }

        public string DateShow
        {
            get
            {
                DateTime dt = new DateTime();
                dt = Convert.ToDateTime(HistoryCall.date_time);
                return dt.ToString("ddd, d MMMM H:mm");
            }
            set
            {
                if (HistoryCall.date_time != value)
                {
                    HistoryCall.date_time = value;
                    OnPropertyChanged("DateShow");
                }
            }
        }

        public string Duration
        {
            get
            {
                string mm = "";
                string ss = "";
                if (Convert.ToInt32(HistoryCall.duration) % 60 > 9)
                    ss += (Convert.ToInt32(HistoryCall.duration) % 60).ToString();
                else
                    ss += "0" + (Convert.ToInt32(HistoryCall.duration) % 60).ToString();
                if (Convert.ToInt32(HistoryCall.duration) / 60 > 9)
                    mm += (Convert.ToInt32(HistoryCall.duration) / 60).ToString();
                else
                    mm += "0" + (Convert.ToInt32(HistoryCall.duration) / 60).ToString();
                return mm + ":" + ss;
            }
            set
            {
                if (HistoryCall.duration != value)
                {
                    HistoryCall.duration = value;
                    OnPropertyChanged("Duration");
                }
            }
        }

        public string Caller
        {
            get
            {
                return HistoryCall.who_call;
            }
            set
            {
                if (HistoryCall.who_call != value)
                {
                    HistoryCall.who_call = value;
                    OnPropertyChanged("Caller");
                }
            }
        }

        public string CallStatus
        {
            get
            {
                return HistoryCall.type;
            }
            set
            {
                if (HistoryCall.type != value)
                {
                    HistoryCall.type = value;
                    OnPropertyChanged("CallStatus");
                }
            }
        }
    }
}