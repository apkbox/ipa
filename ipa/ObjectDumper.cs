// --------------------------------------------------------------------------------
// <copyright file="Class1.cs" company="Alex Kozlov">
//   Copyright (c) Alex Kozlov. All rights reserved.
// </copyright>
// <summary>
//   Defines the ObjectDumper type.
// </summary>
// --------------------------------------------------------------------------------

namespace Ipa
{
    using System;
    using System.Collections;
    using System.IO;
    using System.Reflection;

    // See the ReadMe.html for additional information
    public class ObjectDumper
    {
        #region Fields

        private int depth;

        private int level;

        private int pos;

        private TextWriter writer;

        #endregion

        #region Constructors and Destructors

        private ObjectDumper(int depth)
        {
            this.depth = depth;
        }

        #endregion

        #region Public Methods and Operators

        public static void Write(object element)
        {
            Write(element, 0);
        }

        public static void Write(object element, int depth)
        {
            Write(element, depth, Console.Out);
        }

        public static void Write(object element, int depth, TextWriter log)
        {
            ObjectDumper dumper = new ObjectDumper(depth);
            dumper.writer = log;
            dumper.WriteObject(null, element);
        }

        #endregion

        #region Methods

        private void Write(string s)
        {
            if (s != null)
            {
                this.writer.Write(s);
                this.pos += s.Length;
            }
        }

        private void WriteIndent()
        {
            for (int i = 0; i < this.level; i++)
            {
                this.writer.Write("  ");
            }
        }

        private void WriteLine()
        {
            this.writer.WriteLine();
            this.pos = 0;
        }

        private void WriteObject(string prefix, object element)
        {
            if (element == null || element is ValueType || element is string)
            {
                this.WriteIndent();
                Write(prefix);
                this.WriteValue(element);
                this.WriteLine();
            }
            else
            {
                IEnumerable enumerableElement = element as IEnumerable;
                if (enumerableElement != null)
                {
                    foreach (object item in enumerableElement)
                    {
                        if (item is IEnumerable && !(item is string))
                        {
                            this.WriteIndent();
                            Write(prefix);
                            this.Write("...");
                            this.WriteLine();
                            if (this.level < this.depth)
                            {
                                this.level++;
                                this.WriteObject(prefix, item);
                                this.level--;
                            }
                        }
                        else
                        {
                            this.WriteObject(prefix, item);
                        }
                    }
                }
                else
                {
                    MemberInfo[] members = element.GetType().GetMembers(BindingFlags.Public | BindingFlags.Instance);
                    this.WriteIndent();
                    Write(prefix);
                    bool propWritten = false;
                    foreach (MemberInfo m in members)
                    {
                        FieldInfo f = m as FieldInfo;
                        PropertyInfo p = m as PropertyInfo;
                        if (f != null || p != null)
                        {
                            if (propWritten)
                            {
                                this.WriteTab();
                            }
                            else
                            {
                                propWritten = true;
                            }

                            Write(m.Name);
                            this.Write("=");
                            Type t = f != null ? f.FieldType : p.PropertyType;
                            if (t.IsValueType || t == typeof(string))
                            {
                                this.WriteValue(f != null ? f.GetValue(element) : p.GetValue(element, null));
                            }
                            else
                            {
                                if (typeof(IEnumerable).IsAssignableFrom(t))
                                {
                                    this.Write("...");
                                }
                                else
                                {
                                    this.Write("{ }");
                                }
                            }
                        }
                    }

                    if (propWritten)
                    {
                        this.WriteLine();
                    }

                    if (this.level < this.depth)
                    {
                        foreach (MemberInfo m in members)
                        {
                            FieldInfo f = m as FieldInfo;
                            PropertyInfo p = m as PropertyInfo;
                            if (f != null || p != null)
                            {
                                Type t = f != null ? f.FieldType : p.PropertyType;
                                if (!(t.IsValueType || t == typeof(string)))
                                {
                                    object value = f != null ? f.GetValue(element) : p.GetValue(element, null);
                                    if (value != null)
                                    {
                                        this.level++;
                                        this.WriteObject(m.Name + ": ", value);
                                        this.level--;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void WriteTab()
        {
            this.Write("  ");
            while (this.pos % 8 != 0)
            {
                this.Write(" ");
            }
        }

        private void WriteValue(object o)
        {
            if (o == null)
            {
                this.Write("null");
            }
            else if (o is DateTime)
            {
                this.Write(((DateTime)o).ToShortDateString());
            }
            else if (o is ValueType || o is string)
            {
                Write(o.ToString());
            }
            else if (o is IEnumerable)
            {
                this.Write("...");
            }
            else
            {
                this.Write("{ }");
            }
        }

        #endregion
    }
}
