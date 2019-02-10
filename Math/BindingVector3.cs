using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using System.ComponentModel;

namespace WindEditor
{
    public class BindingVector3 : INotifyPropertyChanged
    {
        // WPF
        public event PropertyChangedEventHandler PropertyChanged;

        public float X
        {
            get { return m_BackingVector.X; }
            set
            {
                if (value != m_BackingVector.X)
                {
                    m_BackingVector.X = value;
                    OnPropertyChanged("X");
                }
            }
        }

        public float Y
        {
            get { return m_BackingVector.Y; }
            set
            {
                if (value != m_BackingVector.Y)
                {
                    m_BackingVector.Y = value;
                    OnPropertyChanged("Y");
                }
            }
        }

        public float Z
        {
            get { return m_BackingVector.Z; }
            set
            {
                if (value != m_BackingVector.Z)
                {
                    m_BackingVector.Z = value;
                    OnPropertyChanged("Z");
                }
            }
        }

        public Vector3 BackingVector
        {
            get { return m_BackingVector; }
            set
            {
                if (value != m_BackingVector)
                {
                    m_BackingVector = value;
                    OnPropertyChanged("BackingVector");
                }
            }
        }

        private Vector3 m_BackingVector;
        private int rand;

        public BindingVector3()
        {
            m_BackingVector = new Vector3();

            Random et = new Random();

            rand = et.Next();
        }

        public BindingVector3(Vector3 v)
        {
            m_BackingVector = v;

            Random et = new Random();

            rand = et.Next();
        }

        public static implicit operator BindingVector3(Vector3 v)
        {
            return new BindingVector3(v);
        }

        public static implicit operator Vector3(BindingVector3 b)
        {
            return b.m_BackingVector;
        }

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
