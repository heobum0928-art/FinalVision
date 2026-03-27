using Project.BaseLib.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Project.BaseLib.DataStructures
{
    //[DataContract]
    //[System.SerializableAttribute()]
    //[IsDerived]
    //public partial class LoadPortCarrierInfo : INotifyPropertyChanged
    //{
    //    #region Fields
    //    private LoadPortInfo _LoadPortInfo;
    //    private CarrierInfo _CarrierInfo;
    //    private Boolean _AmhsPresent;
    //    private E84Info _E84Info;
    //    #endregion

    //    #region Constructors
    //    public LoadPortCarrierInfo()
    //    {
    //    }

    //    public LoadPortCarrierInfo(PersistencyBase b, LoadPortInfo LoadPortInfo, CarrierInfo CarrierInfo, Boolean AmhsPresent, E84Info E84Info)
    //    {
    //        (b as LoadPortCarrierInfo).Copy(this);
    //        this.LoadPortInfo = LoadPortInfo;
    //        this.CarrierInfo = CarrierInfo;
    //        this.AmhsPresent = AmhsPresent;
    //        this.E84Info = E84Info;
    //    }
    //    public LoadPortCarrierInfo(LoadPortInfo LoadPortInfo, CarrierInfo CarrierInfo, Boolean AmhsPresent, E84Info E84Info)
    //    {
    //        this.LoadPortInfo = LoadPortInfo;
    //        this.CarrierInfo = CarrierInfo;
    //        this.AmhsPresent = AmhsPresent;
    //        this.E84Info = E84Info;
    //    }
    //    #endregion

    //    #region Events


    //    #endregion

    //    #region Properties
    //    [DataMember]
    //    [System.Xml.Serialization.XmlElement]
    //    public LoadPortInfo LoadPortInfo
    //    {
    //        get
    //        {
    //            return _LoadPortInfo;
    //        }
    //        set
    //        {
    //            if (_LoadPortInfo != value)
    //            {
    //                LoadPortInfo oldValue = _LoadPortInfo;
    //                if (oldValue != null)
    //                    oldValue.PropertyChanged -= new PropertyChangedEventHandler(this.RaisePropertyChanged);
    //                if (value != null)
    //                    value.PropertyChanged += new PropertyChangedEventHandler(this.RaisePropertyChanged);
    //                _LoadPortInfo = value;
    //                RaisePropertyChanged("LoadPortInfo");
    //            }
    //        }
    //    }

    //    [DataMember]
    //    [System.Xml.Serialization.XmlElement]
    //    public CarrierInfo CarrierInfo
    //    {
    //        get
    //        {
    //            return _CarrierInfo;
    //        }
    //        set
    //        {
    //            if (_CarrierInfo != value)
    //            {
    //                CarrierInfo oldValue = _CarrierInfo;
    //                if (oldValue != null)
    //                    oldValue.PropertyChanged -= new PropertyChangedEventHandler(this.RaisePropertyChanged);
    //                if (value != null)
    //                    value.PropertyChanged += new PropertyChangedEventHandler(this.RaisePropertyChanged);
    //                _CarrierInfo = value;
    //                RaisePropertyChanged("CarrierInfo");
    //            }
    //        }
    //    }

    //    [DataMember]
    //    [System.Xml.Serialization.XmlAttribute]
    //    public Boolean AmhsPresent
    //    {
    //        get
    //        {
    //            return _AmhsPresent;
    //        }
    //        set
    //        {
    //            if (_AmhsPresent != value)
    //            {
    //                _AmhsPresent = value;
    //                RaisePropertyChanged("AmhsPresent");
    //            }
    //        }
    //    }

    //    [DataMember]
    //    [System.Xml.Serialization.XmlElement]
    //    public E84Info E84Info
    //    {
    //        get
    //        {
    //            return _E84Info;
    //        }
    //        set
    //        {
    //            if (_E84Info != value)
    //            {
    //                E84Info oldValue = _E84Info;
    //                if (oldValue != null)
    //                    oldValue.PropertyChanged -= new PropertyChangedEventHandler(this.RaisePropertyChanged);
    //                if (value != null)
    //                    value.PropertyChanged += new PropertyChangedEventHandler(this.RaisePropertyChanged);
    //                _E84Info = value;
    //                RaisePropertyChanged("E84Info");
    //            }
    //        }
    //    }

    //    #endregion

    //    #region Overridden Methods
    //    public override bool Equals(object obj)
    //    {
    //        LoadPortCarrierInfo other = obj as LoadPortCarrierInfo;
    //        if (other == null)
    //            return false;
    //        if (_LoadPortInfo != null && other._LoadPortInfo != null)
    //        {
    //            if (!other._LoadPortInfo.Equals(_LoadPortInfo))
    //                return false;
    //        }
    //        //Here both should be null so they should equal each other, otherwise return false
    //        else if (_LoadPortInfo != other._LoadPortInfo)
    //            return false;
    //        if (_CarrierInfo != null && other._CarrierInfo != null)
    //        {
    //            if (!other._CarrierInfo.Equals(_CarrierInfo))
    //                return false;
    //        }
    //        //Here both should be null so they should equal each other, otherwise return false
    //        else if (_CarrierInfo != other._CarrierInfo)
    //            return false;
    //        if (_AmhsPresent != null && other._AmhsPresent != null)
    //        {
    //            if (!other._AmhsPresent.Equals(_AmhsPresent))
    //                return false;
    //        }
    //        //Here both should be null so they should equal each other, otherwise return false
    //        else if (_AmhsPresent != other._AmhsPresent)
    //            return false;
    //        if (_E84Info != null && other._E84Info != null)
    //        {
    //            if (!other._E84Info.Equals(_E84Info))
    //                return false;
    //        }
    //        //Here both should be null so they should equal each other, otherwise return false
    //        else if (_E84Info != other._E84Info)
    //            return false;

    //        return base.Equals(obj);
    //    }

    //    public override List<ExploredInfo> GetImages(string level)
    //    {
    //        List<ExploredInfo> items = null;
    //        List<ExploredInfo> result = base.GetImages(level);
    //        if (LoadPortInfo != null)
    //        {
    //            items = LoadPortInfo.GetImages(level + ".LoadPortInfo");
    //            if (items.Any())
    //            {
    //                result.AddRange(items);
    //            }
    //        }
    //        if (CarrierInfo != null)
    //        {
    //            items = CarrierInfo.GetImages(level + ".CarrierInfo");
    //            if (items.Any())
    //            {
    //                result.AddRange(items);
    //            }
    //        }
    //        if (E84Info != null)
    //        {
    //            items = E84Info.GetImages(level + ".E84Info");
    //            if (items.Any())
    //            {
    //                result.AddRange(items);
    //            }
    //        }
    //        return result;
    //    }

    //    public override List<ExploredInfo> GetBlobs(string level)
    //    {
    //        List<ExploredInfo> items = null;
    //        List<ExploredInfo> result = base.GetBlobs(level);
    //        if (LoadPortInfo != null)
    //        {
    //            items = LoadPortInfo.GetBlobs(level + ".LoadPortInfo");
    //            if (items.Any())
    //            {
    //                result.AddRange(items);
    //            }
    //        }
    //        if (CarrierInfo != null)
    //        {
    //            items = CarrierInfo.GetBlobs(level + ".CarrierInfo");
    //            if (items.Any())
    //            {
    //                result.AddRange(items);
    //            }
    //        }
    //        if (E84Info != null)
    //        {
    //            items = E84Info.GetBlobs(level + ".E84Info");
    //            if (items.Any())
    //            {
    //                result.AddRange(items);
    //            }
    //        }
    //        return result;
    //    }

    //    public override int GetHashCode()
    //    {
    //        return base.GetHashCode();
    //    }

    //    public void Copy(LoadPortCarrierInfo copy_elem)
    //    {
    //        base.Copy(copy_elem);
    //        if (_LoadPortInfo != null)
    //        {
    //            copy_elem._LoadPortInfo = _LoadPortInfo.Duplicate();
    //        }
    //        if (_CarrierInfo != null)
    //        {
    //            copy_elem._CarrierInfo = _CarrierInfo.Duplicate();
    //        }
    //        copy_elem._AmhsPresent = _AmhsPresent;
    //        if (_E84Info != null)
    //        {
    //            copy_elem._E84Info = _E84Info.Duplicate();
    //        }
    //    }

    //    public override BaseDataStructure Copy()
    //    {
    //        LoadPortCarrierInfo copy_elem = new LoadPortCarrierInfo();
    //        Copy(copy_elem);
    //        return copy_elem;
    //    }

    //    public new LoadPortCarrierInfo Duplicate()
    //    {
    //        return (LoadPortCarrierInfo)this.Copy();
    //    }

    //    public override void Clear()
    //    {
    //        base.Clear();
    //        if (LoadPortInfo != null)
    //            LoadPortInfo.Clear();
    //        if (CarrierInfo != null)
    //            CarrierInfo.Clear();
    //        AmhsPresent = false;
    //        if (E84Info != null)
    //            E84Info.Clear();
    //    }

    //    protected override void AllocAll()
    //    {
    //        base.AllocAll();
    //        LoadPortInfo = LoadPortInfo.CreateNew();
    //        CarrierInfo = CarrierInfo.CreateNew();
    //        AmhsPresent = false;
    //        E84Info = E84Info.CreateNew();
    //    }

    //    public static new LoadPortCarrierInfo CreateNew()
    //    {
    //        LoadPortCarrierInfo temp = new LoadPortCarrierInfo();
    //        temp.AllocAll();
    //        return temp;
    //    }
    //    public override string ToString()
    //    {
    //        return string.Format("LoadPortCarrierInfo: {0} [ LoadPortInfo = {1}; CarrierInfo = {2}; AmhsPresent = {3}; E84Info = {4};]"
    //            , base.ToString()
    //            , _LoadPortInfo
    //            , _CarrierInfo
    //            , _AmhsPresent
    //            , _E84Info
    //            );
    //    }

    //    #endregion
    //}
}
