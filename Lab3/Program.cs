using System;
using System.Numerics;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.IO;
using System.Globalization;
using System.Runtime.InteropServices;

namespace Lab1
{
    struct DataItem
    {
        public double x { get; set; }
        public double y { get; set; }
        public Vector2 vector { get; set; }
        public DataItem(double x, double y, Vector2 vector)
        {
            this.x = x;
            this.y = y;
            this.vector = vector;
        }
        public string ToLongString(string format)
        {
            return $"Coordinates: [{x.ToString(format)}; {y.ToString(format)}]; vector: [{vector.X.ToString(format)}; {vector.Y.ToString(format)}]; Length = {vector.Length().ToString(format)}";
        }
        public override string ToString()
        {
            return $"x = {x}; y = {y}; vector: [{vector}]";
        }
    }

    delegate Vector2 FdblVector2(double x, double y);

    abstract class V3Data : IEnumerable<DataItem>
    {
        public string str { get; protected set; }
        public DateTime date { get; protected set; }
        public V3Data(string str, DateTime date)
        {
            this.str = str;
            this.date = date;
        }
        protected int count;
        public abstract int Count { get; }
        protected double maxDistance;
        public abstract double MaxDistance { get; }
        public abstract string ToLongString(string format);
        public override string ToString()
        {
            return $"str: {str}; date: [{date}]; count = {Count}; maxdistance = {MaxDistance}";
        }
        public abstract IEnumerator<DataItem> GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
        IEnumerator<DataItem> IEnumerable<DataItem>.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }

    class V3DataList : V3Data, IEnumerable<DataItem>
    {
        public List<DataItem> lst { get; }
        public V3DataList(string str, DateTime date) : base(str, date)
        {
            lst = new List<DataItem>();
        }
        public bool Add(DataItem newItem)
        {
            if (lst.Exists(t => t.x == newItem.x && t.y == newItem.y))
                return false;
            else
            {
                lst.Add(newItem);
                return true;
            }
        }
        public int AddDefaults(int nItems, FdblVector2 F)
        {
            int cnt = 0;
            for (int i = 1; i <= nItems; i++)
            {
                DataItem a = new DataItem(i * i, i * i * 2, F(i * i, i * i * 2));
                if (Add(a))
                    cnt++;
            }
            return cnt;
        }
        public override int Count
        {
            get { return lst.Count; }
        }
        public override double MaxDistance
        {
            get
            {
                double md = 0, dx, dy, d;
                for (int i = 0; i < lst.Count; i++)
                {
                    for (int j = i + 1; j < lst.Count; j++)
                    {
                        dx = lst[j].x - lst[i].x;
                        dy = lst[j].y - lst[i].y;
                        d = Math.Sqrt(dx * dx + dy * dy);
                        if (d > md)
                            md = d;
                    }
                }
                return md;
            }
        }
        public override string ToString()
        {
            return $"Typename: {this.GetType()}; {base.ToString()}";
        }
        public override string ToLongString(string format)
        {
            string s = $"Typename: {this.GetType()}; str: {str}; date: [{date}]; count = {Count}; maxdistance = {MaxDistance.ToString(format)}";
            s += "\n" + "List:";
            for (int i = 0; i < lst.Count; i++)
                s += "\n" + lst[i].ToLongString(format);
            return s;
        }
        public override IEnumerator<DataItem> GetEnumerator()
        {
            return lst.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return lst.GetEnumerator();
        }
        IEnumerator<DataItem> IEnumerable<DataItem>.GetEnumerator()
        {
            return lst.GetEnumerator();
        }
        public bool SaveBinary(string filename)
        {
            FileStream fs = null;
            CultureInfo cInfo = new CultureInfo("ru-RU");
            try
            {
                fs = new FileStream(filename, FileMode.OpenOrCreate);
                BinaryWriter writer = new BinaryWriter(fs);
                writer.Write(str);
                writer.Write(date.ToString());
                writer.Write(Count);
                foreach (DataItem item in lst)
                {
                    writer.Write(item.x);
                    writer.Write(item.y);
                    writer.Write(item.vector.X);
                    writer.Write(item.vector.Y);
                }
                writer.Close();
                return true;
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc.Message);
                return false;
            }
            finally
            {
                if (fs != null)
                    fs.Close();
            }
        }
        static public bool LoadBinary(string filename, ref V3DataList v3)
        {
            FileStream fs = null;
            CultureInfo cInfo = new CultureInfo("ru-RU");
            try
            {
                fs = new FileStream(filename, FileMode.Open);
                BinaryReader reader = new BinaryReader(fs);
                string str = reader.ReadString();
                DateTime date = Convert.ToDateTime(reader.ReadString());
                int count = reader.ReadInt32();
                if (v3 == null)
                {
                    v3 = new V3DataList(str, date);
                }
                else
                {
                    v3.str = str;
                    v3.date = date;
                }
                for (int i = 0; i < count; i++)
                {
                    double x = reader.ReadDouble();
                    double y = reader.ReadDouble();
                    float v_x = reader.ReadSingle();
                    float v_y = reader.ReadSingle();
                    Vector2 vct = new Vector2(v_x, v_y);
                    DataItem dt = new DataItem(x, y, vct);
                    v3.lst.Add(dt);
                }
                reader.Close();
                return true;
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc.Message);
                return false;
            }
            finally
            {
                if (fs != null)
                    fs.Close();
            }
        }
    }

    class V3DataArray : V3Data, IEnumerable<DataItem>
    {
        public int n_x { get; private set; }
        public int n_y { get; private set; }
        public double step_x { get; private set; }
        public double step_y { get; private set; }
        public Vector2[] arr_v { get; set; }
        public Vector2[][] int_res { get; private set; }
        public Vector2[,] arr { get; private set; }
        public V3DataArray(string str, DateTime date) : base(str, date)
        {
            arr = new Vector2[0, 0];
        }
        public V3DataArray(string str, DateTime date, int n_x, int n_y, double step_x, double step_y, FdblVector2 F) : base(str, date)
        {
            this.n_x = n_x;
            this.n_y = n_y;
            this.step_x = step_x;
            this.step_y = step_y;
            arr = new Vector2[n_x, n_y];
            for (int i = 0; i < n_x; i++)
            {
                for (int j = 0; j < n_y; j++)
                {
                    arr[i, j] = F(i * step_x, j * step_y);
                }
            }
        }
        public override int Count
        {
            get { return n_x * n_y; }
        }
        public override double MaxDistance
        {
            get
            {
                if (Count == 0)
                    return 0;
                else
                    return Math.Sqrt((n_x - 1) * step_x * (n_x - 1) * step_x + (n_y - 1) * step_y * (n_y - 1) * step_y);
            }
        }
        public override string ToString()
        {
            return $"Typename: {this.GetType()}; {base.ToString()}; n_x = {n_x}; n_y = {n_y}; step_x = {step_x}; step.y = {step_y}";
        }
        public override string ToLongString(string format)
        {
            string s = $"Typename: {this.GetType()}; str: {str}; date: [{date}]; count = {Count}; maxdistance = {MaxDistance.ToString(format)};";
            s += $" n_x = {n_x}; n_y = {n_y}; step_x = {step_x.ToString(format)}; step_y = {step_y.ToString(format)}";
            s += "\n" + "Array:";
            for (int i = 0; i < n_x; i++)
            {
                for (int j = 0; j < n_y; j++)
                {
                    s += $"\nCoordinates: [{(i * step_x).ToString(format)}; {(j * step_y).ToString(format)}]; vector: [{arr[i, j].X.ToString(format)}; {arr[i, j].Y.ToString(format)}]; length = {arr[i, j].Length().ToString(format)}";
                }
            }
            return s;
        }
        public static explicit operator V3DataList(V3DataArray dataArray)
        {
            V3DataList dataList = new V3DataList(dataArray.str, dataArray.date);
            for (int i = 0; i < dataArray.n_x; i++)
            {
                for (int j = 0; j < dataArray.n_y; j++)
                {
                    dataList.Add(new DataItem(i * dataArray.step_x, j * dataArray.step_y, dataArray.arr[i, j]));
                }
            }
            return dataList;
        }
        public override IEnumerator<DataItem> GetEnumerator()
        {
            for (int i = 0; i < n_x; i++)
            {
                for (int j = 0; j < n_y; j++)
                {
                    DataItem dataItem = new DataItem(i * step_x, j * step_y, arr[i, j]);
                    yield return dataItem;
                }
            }
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
        IEnumerator<DataItem> IEnumerable<DataItem>.GetEnumerator()
        {
            return this.GetEnumerator();
        }
        public bool SaveAsText(string filename)
        {
            FileStream fs = null;
            CultureInfo cInfo = new CultureInfo("ru-RU");
            try
            {
                fs = new FileStream(filename, FileMode.OpenOrCreate);
                StreamWriter sw = new StreamWriter(fs);
                sw.WriteLine(str);
                sw.WriteLine(date.ToString());
                sw.WriteLine(n_x.ToString());
                sw.WriteLine(n_y.ToString());
                sw.WriteLine(step_x.ToString());
                sw.WriteLine(step_y.ToString());
                for (int i = 0; i < n_x; i++)
                {
                    for (int j = 0; j < n_y; j++)
                    {
                        sw.WriteLine(arr[i, j].X.ToString());
                        sw.WriteLine(arr[i, j].Y.ToString());
                    }
                }
                sw.Close();
                return true;
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc.Message);
                return false;
            }
            finally
            {
                if (fs != null)
                    fs.Close();
            }
        }
        static public bool LoadAsText(string filename, ref V3DataArray v3)
        {
            FileStream fs = null;
            CultureInfo cInfo = new CultureInfo("ru-RU");
            try
            {
                fs = new FileStream(filename, FileMode.Open);
                StreamReader sr = new StreamReader(fs);
                string str = sr.ReadLine();
                DateTime date = Convert.ToDateTime(sr.ReadLine());
                if (v3 == null)
                {
                    v3 = new V3DataArray(str, date);
                }
                else
                {
                    v3.str = str;
                    v3.date = date;
                }
                v3.n_x = Convert.ToInt32(sr.ReadLine());
                v3.n_y = Convert.ToInt32(sr.ReadLine());
                v3.step_x = Convert.ToDouble(sr.ReadLine());
                v3.step_y = Convert.ToDouble(sr.ReadLine());
                v3.arr = new Vector2[v3.n_x, v3.n_y];
                for (int i = 0; i < v3.n_x; i++)
                {
                    for (int j = 0; j < v3.n_y; j++)
                    {
                        float x = Convert.ToSingle(sr.ReadLine());
                        float y = Convert.ToSingle(sr.ReadLine());
                        v3.arr[i, j] = new Vector2(x, y);
                    }
                }
                sr.Close();
                return true;
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc.Message);
                return false;
            }
            finally
            {
                if (fs != null)
                    fs.Close();
            }
        }
        [DllImport("..\\..\\..\\x64\\Debug\\Dll3.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Global(float[] arr, int nx, int ny, double step, float[] left, float[] right, int n_int, float[] res);
        public bool Integrals(Vector2[] vec)
        {
            arr_v = new Vector2[vec.Length];
            vec.CopyTo(arr_v, 0);
            float[] left = new float[vec.Length];
            float[] right = new float[vec.Length];
            for (int i = 0; i < vec.Length; i++)
            {
                left[i] = vec[i].X;
                right[i] = vec[i].Y;
            }
            int n_int = vec.Length;
            float[] res = new float[n_y * 2 * n_int];
            float[] rep = new float[Count * 2];
            for (int i = 0; i < n_x; i++)
            {
                for (int j = 0; j < n_y; j++)
                {
                    rep[(i * n_y + j) * 2] = arr[i, j].X;
                    rep[(i * n_y + j) * 2 + 1] = arr[i, j].Y;
                }
            }
            int_res = new Vector2[n_int][];
            for (int i = 0; i < n_int; i++)
                int_res[i] = new Vector2[n_y];
            int status = Global(rep, n_x, n_y, step_x, left, right, n_int, res);
            if (status == 0)
            {
                for (int i = 0; i < (n_y * 2 * n_int); i += 2)
                {
                    int_res[i / (n_y * 2)][(i / 2) % n_y].X = res[i];
                    int_res[i / (n_y * 2)][(i / 2) % n_y].Y = res[i + 1];
                }    
                return true;
            }
            else
                return false;
        }
    }



    class V3MainCollection
    {
        protected DataItem? d_max;
        public DataItem? D_max
        {
            get
            {
                if (Count == 0)
                    return null;
                IEnumerable<DataItem> dQuery = lst.OrderByDescending(item => { if (item.Count == 0) { return 0; } return item.Max(x => Math.Sqrt(x.x * x.x + x.y * x.y)); }).First();
                DataItem dt = dQuery.OrderByDescending(x => Math.Sqrt(x.x * x.x + x.y * x.y)).First();
                return dt;
            }
        }
        protected IEnumerable<double> dup;
        public IEnumerable<double> Dup
        {
            get
            {
                if (Count == 0)
                    return null;
                IEnumerable<DataItem> dQuery1 = lst.Aggregate<IEnumerable<DataItem>>((ls, x) => ls.Concat(x));
                IEnumerable<DataItem> dQuery2 = dQuery1.Where(a => dQuery1.Count(b => b.x == a.x && b.y == a.y) > 1);
                IEnumerable<double> res = (from item in dQuery2 select item.x).Distinct();
                if (res.Count() == 0)
                    return null;
                return res;
            }
        }
        protected IEnumerable<V3Data> date_min;
        public IEnumerable<V3Data> Date_min
        {
            get
            {
                if (Count == 0)
                    return null;
                DateTime d_m = lst.Min(x => x.date);
                IEnumerable<V3Data> dQuery = lst.Where(x => x.date == d_m);
                return dQuery;
            }
        }
        private List<V3Data> lst;
        protected int count;
        public int Count
        {
            get { return lst.Count; }
        }
        public V3MainCollection()
        {
            lst = new List<V3Data>();
        }
        public V3Data this[int index]
        {
            get { return lst[index]; }
        }
        public bool Contains(string ID)
        {
            if (lst.Exists(t => t.str == ID))
                return true;
            else
                return false;
        }
        public bool Add(V3Data v3Data)
        {
            if (Contains(v3Data.str))
                return false;
            else
            {
                lst.Add(v3Data);
                return true;
            }
        }
        public string ToLongString(string format)
        {
            string s = "";
            for (int i = 0; i < Count - 1; i++)
                s += lst[i].ToLongString(format) + "\n";
            s += lst[Count - 1].ToLongString(format);
            return s;
        }
        public override string ToString()
        {
            string s = "";
            for (int i = 0; i < Count - 1; i++)
                s += lst[i].ToString() + "\n";
            s += lst[Count - 1].ToString();
            return s;
        }
    }

    static class Methods
    {
        public static Vector2 F1(double x, double y)
        {
            return new Vector2((float)(x * x * x + 1), (float)(y * y * y + 2));
        }
        public static Vector2 F2(double x, double y)
        {
            return new Vector2((float)(x * 4), (float)(y * 5));
        }
        public static Vector2 F3(double x, double y)
        {
            return new Vector2((float)(x * 5), (float)(y * 6));
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            V3DataArray a = new V3DataArray("ID_1", DateTime.Now, 10, 1, 0.5, 1, Methods.F1);
            float[] left = { 0, 1, 0 };
            float[] right = { 1, 2, 2 };
            Vector2[] vec = new Vector2[left.Length];
            for (int i = 0; i < left.Length; i++)
            {
                vec[i].X = left[i];
                vec[i].Y = right[i];
            }
            Console.WriteLine($"Статус: {a.Integrals(vec)}");
            Console.WriteLine();
            Console.WriteLine("Результаты интегрирования:");
            for (int i = 0; i < left.Length; i++)
            {
                for (int j = 0; j < a.n_y; j++)
                {
                    Console.WriteLine($"Интеграл {i + 1}: {a.int_res[i][j]}");
                }
            }
        }
    }
}

