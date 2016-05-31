using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using UnityEngine;

namespace common
{
    // proto对象 序列化/反序列化 接口
    public interface IMessage
    {
        int Serialize(ref MemoryStream out_stream);

        int ParseFrom(ref MemoryStream int_stream);
    }

    // 基础数据类型 序列化/反序列化
    public class ProtoReadWrite
    {
        public static int Variant(bool in_value, ref MemoryStream out_stream)
        {
            byte temp = (byte)(in_value ? 1 : 0);
            temp = (byte)(temp & 0x7F);
           // Debug.Log("========temp========："+temp);
            out_stream.WriteByte(temp);

            return 1;
        }

        public static int DeVariant(ref bool out_value, ref MemoryStream in_stream)
        {
            if (in_stream.Position >= in_stream.Length)
            {
                out_value = false; 
                return 0;
            }

            int read_byte = in_stream.ReadByte() & 0xff;
            out_value = (read_byte == 0) ? false : true;
            return 1;
        }

        public static int Variant(uint in_value, ref MemoryStream out_stream)
        {
            byte temp = 0;
            int count = 0;
            do
            {
                temp = (byte)((in_value & 0x7F) | 0x80);
                count++;
                if ((in_value >>= 7) != 0)
                {
                    out_stream.WriteByte(temp);
                }
                else
                {
                    temp = (byte)(temp & 0x7F);
                    out_stream.WriteByte(temp);
                    break;
                }
            } while (true);

            return count;
        }

        public static int DeVariant(ref uint out_value, ref MemoryStream in_stream)
        {
            if (in_stream.Position >= in_stream.Length)
            {
                out_value = 0; 
                return 0;
            }
            
            uint read_byte = 0;
            out_value = 0;

            read_byte = ((uint)in_stream.ReadByte()) & 0xff;
            out_value = read_byte & 0x7F;
            if ((read_byte & 0x80) == 0)
            {
                return 1;
            }

            read_byte = ((uint)in_stream.ReadByte()) & 0xff;
            out_value = out_value | ((read_byte & 0x7F) << 7);
            if ((read_byte & 0x80) == 0)
            {
                return 2;
            }

            read_byte = ((uint)in_stream.ReadByte()) & 0xff;
            out_value = out_value | (read_byte & 0x7F) << 14;
            if ((read_byte & 0x80) == 0)
            {
                return 3;
            }

            read_byte = ((uint)in_stream.ReadByte()) & 0xff;
            out_value = out_value | (read_byte & 0x7F) << 21;
            if ((read_byte & 0x80) == 0)
            {
                return 4;
            }

            read_byte = ((uint)in_stream.ReadByte()) & 0xff;
            out_value = out_value | (read_byte & 0x7F) << 28;
            return 5;
        }

        public static int Variant(int in_value, ref MemoryStream out_stream)
        {
            long temp_value = in_value;

            byte temp = 0;
            int count = 0;
            do
            {
                temp = (byte)((temp_value & 0x7F) | 0x80);
                count++;
                if ((temp_value = ((temp_value >>= 7) & 0x1ffffffffffffff)) != 0)
                {
                    out_stream.WriteByte(temp);
                }
                else
                {
                    temp = (byte)(temp & 0x7F);
                    out_stream.WriteByte(temp);
                    break;
                }
            } while (true);

            return count;
        }

        public static int DeVariant(ref int out_value, ref MemoryStream in_stream)
        {
            if (in_stream.Position >= in_stream.Length)
            {
                out_value = 0;
                return 0;
            } 
            
            long temp_value = 0;

            long read_byte = 0;
            out_value = 0;

            read_byte = ((long)in_stream.ReadByte()) & 0xff;
            temp_value = read_byte & 0x7F;
            if ((read_byte & 0x80) == 0)
            {
                out_value = (int)temp_value;
                return 1;
            }

            read_byte = ((long)in_stream.ReadByte()) & 0xff;
            temp_value = temp_value | ((read_byte & 0x7F) << 7);
            if ((read_byte & 0x80) == 0)
            {
                out_value = (int)temp_value;
                return 2;
            }

            read_byte = ((long)in_stream.ReadByte()) & 0xff;
            temp_value = temp_value | ((read_byte & 0x7F) << 14);
            if ((read_byte & 0x80) == 0)
            {
                out_value = (int)temp_value;
                return 3;
            }

            read_byte = ((long)in_stream.ReadByte()) & 0xff;
            temp_value = temp_value | ((read_byte & 0x7F) << 21);
            if ((read_byte & 0x80) == 0)
            {
                out_value = (int)temp_value;
                return 4;
            }

            read_byte = ((long)in_stream.ReadByte()) & 0xff;
            temp_value = temp_value | ((read_byte & 0x7F) << 28);
            if ((read_byte & 0x80) == 0)
            {
                out_value = (int)temp_value;
                return 5;
            }

            read_byte = ((long)in_stream.ReadByte()) & 0xff;
            temp_value = temp_value | ((read_byte & 0x7F) << 35);
            if ((read_byte & 0x80) == 0)
            {
                out_value = (int)temp_value;
                return 6;
            }

            read_byte = ((long)in_stream.ReadByte()) & 0xff;
            temp_value = temp_value | ((read_byte & 0x7F) << 42);
            if ((read_byte & 0x80) == 0)
            {
                out_value = (int)temp_value;
                return 7;
            }

            read_byte = ((long)in_stream.ReadByte()) & 0xff;
            temp_value = temp_value | ((read_byte & 0x7F) << 49);
            if ((read_byte & 0x80) == 0)
            {
                out_value = (int)temp_value;
                return 8;
            }

            read_byte = ((long)in_stream.ReadByte()) & 0xff;
            temp_value = temp_value | ((read_byte & 0x7F) << 56);
            if ((read_byte & 0x80) == 0)
            {
                out_value = (int)temp_value;
                return 9;
            }

            read_byte = ((long)in_stream.ReadByte()) & 0xff;
            temp_value = temp_value | ((read_byte & 0x7F) << 63);
            out_value = (int)temp_value;
            return 10;
        }

        public static int Variant(ulong in_value, ref MemoryStream out_stream)
        {
            byte temp = 0;
            int count = 0;
            do
            {
                temp = (byte)((in_value & 0x7F) | 0x80);
                count++;
                if ((in_value >>= 7) != 0)
                {
                    out_stream.WriteByte(temp);
                }
                else
                {
                    temp = (byte)(temp & 0x7F);
                    out_stream.WriteByte(temp);
                    break;
                }
            } while (true);

            return count;
        }

        public static int DeVariant(ref ulong out_value, ref MemoryStream in_stream)
        {
            if (in_stream.Position >= in_stream.Length)
            {
                out_value = 0;
                return 0;
            } 
            
            ulong read_byte = 0;
            out_value = 0;

            read_byte = ((ulong)in_stream.ReadByte()) & 0xff;
            out_value = read_byte & 0x7F;
            if ((read_byte & 0x80) == 0)
            {
                return 1;
            }

            read_byte = ((ulong)in_stream.ReadByte()) & 0xff;
            out_value = out_value | ((read_byte & 0x7F) << 7);
            if ((read_byte & 0x80) == 0)
            {
                return 2;
            }

            read_byte = ((ulong)in_stream.ReadByte()) & 0xff;
            out_value = out_value | ((read_byte & 0x7F) << 14);
            if ((read_byte & 0x80) == 0)
            {
                return 3;
            }

            read_byte = ((ulong)in_stream.ReadByte()) & 0xff;
            out_value = out_value | ((read_byte & 0x7F) << 21);
            if ((read_byte & 0x80) == 0)
            {
                return 4;
            }

            read_byte = ((ulong)in_stream.ReadByte()) & 0xff;
            out_value = out_value | ((read_byte & 0x7F) << 28);
            if ((read_byte & 0x80) == 0)
            {
                return 5;
            }

            read_byte = ((ulong)in_stream.ReadByte()) & 0xff;
            out_value = out_value | ((read_byte & 0x7F) << 35);
            if ((read_byte & 0x80) == 0)
            {
                return 6;
            }

            read_byte = ((ulong)in_stream.ReadByte()) & 0xff;
            out_value = out_value | ((read_byte & 0x7F) << 42);
            if ((read_byte & 0x80) == 0)
            {
                return 7;
            }

            read_byte = ((ulong)in_stream.ReadByte()) & 0xff;
            out_value = out_value | ((read_byte & 0x7F) << 49);
            if ((read_byte & 0x80) == 0)
            {
                return 8;
            }

            read_byte = ((ulong)in_stream.ReadByte()) & 0xff;
            out_value = out_value | ((read_byte & 0x7F) << 56);
            if ((read_byte & 0x80) == 0)
            {
                return 9;
            }

            read_byte = ((ulong)in_stream.ReadByte()) & 0xff;
            out_value = out_value | ((read_byte & 0x7F) << 63);
            return 10;
        }

        public static int Variant(long in_value, ref MemoryStream out_stream)
        {
            byte temp = 0;
            int count = 0;
            do
            {
                temp = (byte)((in_value & 0x7F) | 0x80);
                count++;
                if ((in_value = ((in_value >>= 7) & 0x1ffffffffffffff)) != 0)
                {
                    out_stream.WriteByte(temp);
                }
                else
                {
                    temp = (byte)(temp & 0x7F);
                    out_stream.WriteByte(temp);
                    break;
                }
            } while (true);

            return count;
        }

        public static int DeVariant(ref long out_value, ref MemoryStream in_stream)
        {
            if (in_stream.Position >= in_stream.Length)
            {
                out_value = 0;
                return 0;
            } 
            
            long read_byte = 0;
            out_value = 0;

            read_byte = ((long)in_stream.ReadByte()) & 0xff;
            out_value = read_byte & 0x7F;
            if ((read_byte & 0x80) == 0)
            {
                return 1;
            }

            read_byte = ((long)in_stream.ReadByte()) & 0xff;
            out_value = out_value | ((read_byte & 0x7F) << 7);
            if ((read_byte & 0x80) == 0)
            {
                return 2;
            }

            read_byte = ((long)in_stream.ReadByte()) & 0xff;
            out_value = out_value | ((read_byte & 0x7F) << 14);
            if ((read_byte & 0x80) == 0)
            {
                return 3;
            }

            read_byte = ((long)in_stream.ReadByte()) & 0xff;
            out_value = out_value | ((read_byte & 0x7F) << 21);
            if ((read_byte & 0x80) == 0)
            {
                return 4;
            }

            read_byte = ((long)in_stream.ReadByte()) & 0xff;
            out_value = out_value | ((read_byte & 0x7F) << 28);
            if ((read_byte & 0x80) == 0)
            {
                return 5;
            }

            read_byte = ((long)in_stream.ReadByte()) & 0xff;
            out_value = out_value | ((read_byte & 0x7F) << 35);
            if ((read_byte & 0x80) == 0)
            {
                return 6;
            }

            read_byte = ((long)in_stream.ReadByte()) & 0xff;
            out_value = out_value | ((read_byte & 0x7F) << 42);
            if ((read_byte & 0x80) == 0)
            {
                return 7;
            }

            read_byte = ((long)in_stream.ReadByte()) & 0xff;
            out_value = out_value | ((read_byte & 0x7F) << 49);
            if ((read_byte & 0x80) == 0)
            {
                return 8;
            }

            read_byte = ((long)in_stream.ReadByte()) & 0xff;
            out_value = out_value | ((read_byte & 0x7F) << 56);
            if ((read_byte & 0x80) == 0)
            {
                return 9;
            }

            read_byte = ((long)in_stream.ReadByte()) & 0xff;
            out_value = out_value | ((read_byte & 0x7F) << 63);
            return 10;
        }

        public static int Code(string in_value, ref MemoryStream out_stream)
        {
            byte[] byteArray = System.Text.Encoding.UTF8.GetBytes(in_value);

            for (int index = 0; index < byteArray.Length; index++)
            {
                out_stream.WriteByte(byteArray[index]);
            }

            return byteArray.Length;
        }

        public static int DeCode(ref string out_value, ref MemoryStream in_stream, int len)
        {
            if (in_stream.Position >= in_stream.Length)
            {
                out_value = "";
                return 0;
            } 
            
            byte[] byteArray = new byte[len];

            for (int index = 0; index < byteArray.Length; index++)
            {
                byteArray[index] = (byte)in_stream.ReadByte();
            }
            out_value = System.Text.Encoding.UTF8.GetString(byteArray, 0, byteArray.Length);

            return byteArray.Length;
        }

        public static int Code(byte[] out_value, ref MemoryStream out_stream)
        {
            out_stream.Write(out_value, 0, out_value.Length);

            return out_value.Length;
        }

        public static int DeCode(ref byte[] out_value, ref MemoryStream in_stream, int len)
        {
            if (in_stream.Position >= in_stream.Length)
            {
                return 0;
            }

            out_value = new byte[len];
            in_stream.Read(out_value, 0, len);

            return out_value.Length;
        }

        public static int Float(float in_value, ref MemoryStream out_stream)
        {
            byte[] b = BitConverter.GetBytes(in_value);

            for (int i = 0; i < b.Length; i++)
            {
                out_stream.WriteByte(b[i]);
            }

            return b.Length;
        }

        public static int DeFloat(ref float in_value, ref MemoryStream in_stream)
        {
            byte[] b = new byte[4];
            b[0] = (byte)in_stream.ReadByte();
            b[1] = (byte)in_stream.ReadByte();
            b[2] = (byte)in_stream.ReadByte();
            b[3] = (byte)in_stream.ReadByte();

            in_value = BitConverter.ToSingle(b, 0);

            return 4;
        }
    }

    public class ProtoMemberBase
    {
        private uint m_member_id;     // 字段序号
        private bool m_is_required;   // 是否是必填字段
        private bool m_is_embedded;   // 是否是嵌套子结构
        private bool m_has_value;     // 是否填值

        public ProtoMemberBase(uint member_id, bool is_required, bool is_embedded)
        {
            m_member_id = member_id;
            m_is_required = is_required;
            m_is_embedded = is_embedded;
            m_has_value = false;
        }

        public ProtoMemberBase(ref ProtoMemberBase other)
        {
            m_member_id = other.m_member_id;
            m_is_required = other.m_is_required;
            m_is_embedded = other.m_is_embedded;
            m_has_value = false;
        }

        public uint member_id
        {
            get { return m_member_id; }
            set { m_member_id = value; }
        }

        public bool is_required
        {
            get { return m_is_required; }
            set { m_is_required = value; }
        }

        public bool is_embedded
        {
            get { return m_is_embedded; }
            set { m_is_embedded = value; }
        }

        public bool has_value
        {
            get { return m_has_value; }
            set { m_has_value = value; }
        }
    }

    public class ProtoMemberBytesReadWrite : ProtoMemberBase
    {
        public ProtoMemberBytesReadWrite(uint member_id, bool is_required) :
            base(member_id, is_required, false)
        {

        }

        public int Serialize(byte[] member_value, ref MemoryStream out_stream)
        {
            int count = 0;

            // 是必填字段， 或者选填字段有值
            if (is_required || has_value)
            {
                // 先解析字符串内容，并得到内容的实际长度
                MemoryStream temp = new MemoryStream();
                int len = ProtoReadWrite.Code(member_value, ref temp);

                if (0 < len)
                {
                    // 写key：m_member_id << 3 | 2（字符串对应类型）;
                    uint type = member_id << 3 | 2;
                    count += ProtoReadWrite.Variant(type, ref out_stream);

                    // 写长度len
                    count += ProtoReadWrite.Variant(len, ref out_stream);

                    // 写实际内容
                    temp.WriteTo(out_stream);
                    count += (int)temp.Length;
                }
            }

            return count;
        }

        public int ParseFrom(ref byte[] member_value, ref MemoryStream int_stream)
        {
            int count = 0;

            // 读key
            uint type = 0;
            count += ProtoReadWrite.DeVariant(ref type, ref int_stream);

            // key中的字段序号匹配，否则恢复int_stream并跳过
            uint temp_member_id = type >> 3;
            if (temp_member_id == member_id)
            {
                // 读长度len
                int len = 0;
                count += ProtoReadWrite.DeVariant(ref len, ref int_stream);

                if (0 < len)
                {
                    // 读内容
                    count += ProtoReadWrite.DeCode(ref member_value, ref int_stream, len);

                    // 标记有值
                    has_value = true;
                }
            }
            else
            {
                // 恢复流的当前位置
                long curr_stream_len = int_stream.Position;
                int_stream.Seek(curr_stream_len - count, SeekOrigin.Begin);

                count = 0;
            }

            return count;
        }
    }

    public class ProtoMemberBytes : ProtoMemberBytesReadWrite
    {
        private byte[] m_member_value;

        public ProtoMemberBytes(uint member_id, bool is_required) :
            base(member_id, is_required)
        {
            m_member_value = new byte[1];
        }

        public byte[] member_value
        {
            get { return m_member_value; }
            set { m_member_value = value; has_value = true; }
        }
    }

    public class ProtoMemberStringReadWrite : ProtoMemberBase
    {
        public ProtoMemberStringReadWrite(uint member_id, bool is_required) :
            base(member_id, is_required, false)
        {

        }

        public int Serialize(string member_value, ref MemoryStream out_stream)
        {
            int count = 0;

            // 是必填字段， 或者选填字段有值
            if (is_required || has_value)
            {
                // 先解析字符串内容，并得到内容的实际长度
                MemoryStream temp = new MemoryStream();
                int len = ProtoReadWrite.Code(member_value, ref temp);

                if (0 < len)
                {
                    // 写key：m_member_id << 3 | 2（字符串对应类型）;
                    uint type = member_id << 3 | 2;
                    count += ProtoReadWrite.Variant(type, ref out_stream);

                    // 写长度len
                    count += ProtoReadWrite.Variant(len, ref out_stream);

                    // 写实际内容
                    temp.WriteTo(out_stream);
                    count += (int)temp.Length;
                }
            }

            return count;
        }

        public int ParseFrom(ref string member_value, ref MemoryStream int_stream)
        {
            int count = 0;

            // 读key
            uint type = 0;
            count += ProtoReadWrite.DeVariant(ref type, ref int_stream);

            // key中的字段序号匹配，否则恢复int_stream并跳过
            uint temp_member_id = type >> 3;
            if (temp_member_id == member_id)
            {
                // 读长度len
                int len = 0;
                count += ProtoReadWrite.DeVariant(ref len, ref int_stream);

                if (0 < len)
                {
                    // 读内容
                    count += ProtoReadWrite.DeCode(ref member_value, ref int_stream, len);

                    // 标记有值
                    has_value = true;
                }
            }
            else
            {
                // 恢复流的当前位置
                long curr_stream_len = int_stream.Position;
                int_stream.Seek(curr_stream_len - count, SeekOrigin.Begin);

                count = 0;
            }

            return count;
        }
    }

    public class ProtoMemberString : ProtoMemberStringReadWrite
    {
        private string m_member_value;

        public ProtoMemberString(uint member_id, bool is_required) :
            base(member_id, is_required)
        {
            m_member_value = "";
        }

        public string member_value
        {
            get { return m_member_value; }
            set { m_member_value = value; has_value = true; }
        }
    }

    public class ProtoMemberStringList : ProtoMemberStringReadWrite
    {
        private System.Collections.Generic.List<string> m_member_value;

        public ProtoMemberStringList(uint member_id, bool is_required) :
            base(member_id, true)
        {
            m_member_value = new System.Collections.Generic.List<string>();
        }

        public System.Collections.Generic.List<string> member_value
        {
            get { return m_member_value; }
        }
    }

    public class ProtoMemberEmbeddedReadWrite : ProtoMemberBase
    {
        public ProtoMemberEmbeddedReadWrite(uint member_id, bool is_required) :
            base(member_id, is_required, true)
        {

        }

        public int Serialize(IMessage member_value, ref MemoryStream out_stream)
        {
            int count = 0;

            // 是必填字段， 或者选填字段有值
            if (is_required || has_value)
            {
                // 先解析字符串内容，并得到内容的实际长度
                MemoryStream temp = new MemoryStream();
                int len = member_value.Serialize(ref temp);

                if (0 < len)
                {
                    // 写key：m_member_id << 3 | 2（嵌套结构对应类型）;
                    uint type = member_id << 3 | 2;
                    count += ProtoReadWrite.Variant(type, ref out_stream);

                    // 写长度len
                    count += ProtoReadWrite.Variant(len, ref out_stream);

                    // 写实际内容
                    temp.WriteTo(out_stream);
                    count += (int)temp.Length;
                }
            }

            return count;
        }

        public int ParseFrom(IMessage member_value, ref MemoryStream int_stream)
        {
            int count = 0;

            // 读key
            uint type = 0;
            count += ProtoReadWrite.DeVariant(ref type, ref int_stream);

            // key中的字段序号匹配，否则恢复int_stream并跳过
            uint temp_member_id = type >> 3;
            if (temp_member_id == member_id)
            {
                // 读长度len
                int len = 0;
                count += ProtoReadWrite.DeVariant(ref len, ref int_stream);

                if (0 < len)
                {
                    // 读内容
                    MemoryStream temp_stream = new MemoryStream();
                    temp_stream.Write(int_stream.GetBuffer(), (int)int_stream.Position, len);
                    temp_stream.Seek(0, SeekOrigin.Begin);

                    count += member_value.ParseFrom(ref temp_stream);

                    int_stream.Seek(len, SeekOrigin.Current);

                    // 标记有值
                    has_value = true;
                }
            }
            else
            {
                // 恢复流的当前位置
                long curr_stream_len = int_stream.Position;
                int_stream.Seek(curr_stream_len - count, SeekOrigin.Begin);

                count = 0;
            }

            return count;
        }
    }

    public class ProtoMemberEmbedded<T> : ProtoMemberEmbeddedReadWrite
    {
        private T m_member_value;

        public ProtoMemberEmbedded(uint member_id, bool is_required) :
            base(member_id, is_required)
        {
            m_member_value = default(T);
        }

        public T member_value
        {
            get { return m_member_value; }
            set { m_member_value = value; has_value = true; }
        }
    }

    public class ProtoMemberEmbeddedList<T> : ProtoMemberEmbeddedReadWrite
    {
        private System.Collections.Generic.List<T> m_member_value;

        public ProtoMemberEmbeddedList(uint member_id, bool is_required) :
            base(member_id, true)
        {
            m_member_value = new System.Collections.Generic.List<T>();
        }

        public System.Collections.Generic.List<T> member_value
        {
            get { return m_member_value; }
        }
    }

    public class ProtoMemberUInt32ReadWrite : ProtoMemberBase
    {
        public ProtoMemberUInt32ReadWrite(uint member_id, bool is_required) :
            base(member_id, is_required, false)
        {

        }

        public int Serialize(uint member_value, ref MemoryStream out_stream)
        {
            int count = 0;

            // 是必填字段， 或者选填字段有值
            if (is_required || has_value)
            {
                // 写key：m_member_id << 3 | 0（uint对应类型）;
                uint type = member_id << 3 | 0;
                count += ProtoReadWrite.Variant(type, ref out_stream);

                // 写value
                count += ProtoReadWrite.Variant(member_value, ref out_stream);
            }

            return count;
        }

        public int ParseFrom(ref uint member_value, ref MemoryStream int_stream)
        {
            int count = 0;

            // 读key
            uint type = 0;
            count += ProtoReadWrite.DeVariant(ref type, ref int_stream);

            // key中的字段序号匹配，否则恢复int_stream并跳过
            uint temp_member_id = type >> 3;
            if (temp_member_id == member_id)
            {
                // 读value
                count += ProtoReadWrite.DeVariant(ref member_value, ref int_stream);

                // 标记有值
                has_value = true;
            }
            else
            {
                // 恢复流的当前位置
                long curr_stream_len = int_stream.Position;
                int_stream.Seek(curr_stream_len - count, SeekOrigin.Begin);

                count = 0;
            }

            return count;
        }
    }

    public class ProtoMemberUInt32 : ProtoMemberUInt32ReadWrite
    {
        private uint m_member_value;

        public ProtoMemberUInt32(uint member_id, bool is_required) :
            base(member_id, is_required)
        {
            m_member_value = 0;
        }

        public uint member_value
        {
            get { return m_member_value; }
            set { m_member_value = value; has_value = true; }
        }
    }

    public class  ProtoMemberEnum<T> : ProtoMemberUInt32ReadWrite
    {
        private T m_member_value;

        public ProtoMemberEnum(uint member_id, bool is_required) :
            base(member_id, is_required)
        {
            m_member_value = default(T);
        }

        public T member_value
        {
            get { return m_member_value; }
            set { m_member_value = value; has_value = true; }
        }
    }

    public class ProtoMemberUInt32List : ProtoMemberUInt32ReadWrite
    {
        private System.Collections.Generic.List<uint> m_member_value;

        public ProtoMemberUInt32List(uint member_id, bool is_required) :
            base(member_id, true)
        {
            m_member_value = new System.Collections.Generic.List<uint>();
        }

        public System.Collections.Generic.List<uint> member_value
        {
            get { return m_member_value; }
        }
    }

    public class ProtoMemberEnumList<T> : ProtoMemberUInt32ReadWrite
    {
        private System.Collections.Generic.List<T> m_member_value;

        public ProtoMemberEnumList(uint member_id, bool is_required) :
            base(member_id, is_required)
        {
            m_member_value = new System.Collections.Generic.List<T>();
        }

        public System.Collections.Generic.List<T> member_value
        {
            get { return m_member_value; }
        }
    }

    public class ProtoMemberInt32ReadWrite : ProtoMemberBase
    {
        public ProtoMemberInt32ReadWrite(uint member_id, bool is_required) :
            base(member_id, is_required, false)
        {

        }

        public int Serialize(int member_value, ref MemoryStream out_stream)
        {
            int count = 0;

            // 是必填字段， 或者选填字段有值
            if (is_required || has_value)
            {
                // 写key：m_member_id << 3 | 0（uint对应类型）;
                uint type = member_id << 3 | 0;
                count += ProtoReadWrite.Variant(type, ref out_stream);

                // 写value
                count += ProtoReadWrite.Variant(member_value, ref out_stream);
            }

            return count;
        }

        public int ParseFrom(ref int member_value, ref MemoryStream int_stream)
        {
            int count = 0;

            // 读key
            uint type = 0;
            count += ProtoReadWrite.DeVariant(ref type, ref int_stream);

            // key中的字段序号匹配，否则恢复int_stream并跳过
            uint temp_member_id = type >> 3;
            if (temp_member_id == member_id)
            {
                // 读value
                count += ProtoReadWrite.DeVariant(ref member_value, ref int_stream);

                // 标记有值
                has_value = true;
            }
            else
            {
                // 恢复流的当前位置
                long curr_stream_len = int_stream.Position;
                int_stream.Seek(curr_stream_len - count, SeekOrigin.Begin);

                count = 0;
            }

            return count;
        }
    }

    public class ProtoMemberInt32 : ProtoMemberInt32ReadWrite
    {
        private int m_member_value;

        public ProtoMemberInt32(uint member_id, bool is_required) :
            base(member_id, is_required)
        {
            m_member_value = 0;
        }

        public int member_value
        {
            get { return m_member_value; }
            set { m_member_value = value; has_value = true; }
        }
    }

    public class ProtoMemberInt32List : ProtoMemberInt32ReadWrite
    {
        private System.Collections.Generic.List<int> m_member_value;

        public ProtoMemberInt32List(uint member_id, bool is_required) :
            base(member_id, true)
        {
            m_member_value = new System.Collections.Generic.List<int>();
        }

        public System.Collections.Generic.List<int> member_value
        {
            get { return m_member_value; }
        }
    }

    public class ProtoMemberUInt64ReadWrite : ProtoMemberBase
    {
        public ProtoMemberUInt64ReadWrite(uint member_id, bool is_required) :
            base(member_id, is_required, false)
        {

        }

        public int Serialize(ulong member_value, ref MemoryStream out_stream)
        {
            int count = 0;

            // 是必填字段， 或者选填字段有值
            if (is_required || has_value)
            {
                // 写key：m_member_id << 3 | 0（uint对应类型）;
                uint type = member_id << 3 | 0;
                count += ProtoReadWrite.Variant(type, ref out_stream);

                // 写value
                count += ProtoReadWrite.Variant(member_value, ref out_stream);
            }

            return count;
        }

        public int ParseFrom(ref ulong member_value, ref MemoryStream int_stream)
        {
            int count = 0;

            // 读key
            uint type = 0;
            count += ProtoReadWrite.DeVariant(ref type, ref int_stream);

            // key中的字段序号匹配，否则恢复int_stream并跳过
            uint temp_member_id = type >> 3;
            if (temp_member_id == member_id)
            {
                // 读value
                count += ProtoReadWrite.DeVariant(ref member_value, ref int_stream);

                // 标记有值
                has_value = true;
            }
            else
            {
                // 恢复流的当前位置
                long curr_stream_len = int_stream.Position;
                int_stream.Seek(curr_stream_len - count, SeekOrigin.Begin);

                count = 0;
            }

            return count;
        }
    }

    public class ProtoMemberUInt64 : ProtoMemberUInt64ReadWrite
    {
        private ulong m_member_value;

        public ProtoMemberUInt64(uint member_id, bool is_required) :
            base(member_id, is_required)
        {
            m_member_value = 0;
        }

        public ulong member_value
        {
            get { return m_member_value; }
            set { m_member_value = value; has_value = true; }
        }
    }

    public class ProtoMemberUInt64List : ProtoMemberUInt64ReadWrite
    {
        private System.Collections.Generic.List<ulong> m_member_value;

        public ProtoMemberUInt64List(uint member_id, bool is_required) :
            base(member_id, true)
        {
            m_member_value = new System.Collections.Generic.List<ulong>();
        }

        public System.Collections.Generic.List<ulong> member_value
        {
            get { return m_member_value; }
        }
    }

    public class ProtoMemberInt64ReadWrite : ProtoMemberBase
    {
        public ProtoMemberInt64ReadWrite(uint member_id, bool is_required) :
            base(member_id, is_required, false)
        {

        }

        public int Serialize(long member_value, ref MemoryStream out_stream)
        {
            int count = 0;

            // 是必填字段， 或者选填字段有值
            if (is_required || has_value)
            {
                // 写key：m_member_id << 3 | 0（long对应类型）;
                uint type = member_id << 3 | 0;
                count += ProtoReadWrite.Variant(type, ref out_stream);

                // 写value
                count += ProtoReadWrite.Variant(member_value, ref out_stream);
            }

            return count;
        }

        public int ParseFrom(ref long member_value, ref MemoryStream int_stream)
        {
            int count = 0;

            // 读key
            uint type = 0;
            count += ProtoReadWrite.DeVariant(ref type, ref int_stream);

            // key中的字段序号匹配，否则恢复int_stream并跳过
            uint temp_member_id = type >> 3;
            if (temp_member_id == member_id)
            {
                // 读value
                count += ProtoReadWrite.DeVariant(ref member_value, ref int_stream);

                // 标记有值
                has_value = true;
            }
            else
            {
                // 恢复流的当前位置
                long curr_stream_len = int_stream.Position;
                int_stream.Seek(curr_stream_len - count, SeekOrigin.Begin);

                count = 0;
            }

            return count;
        }
    }

    public class ProtoMemberInt64 : ProtoMemberInt64ReadWrite
    {
        private long m_member_value;

        public ProtoMemberInt64(uint member_id, bool is_required) :
            base(member_id, is_required)
        {
            m_member_value = 0;
        }

        public long member_value
        {
            get { return m_member_value; }
            set { m_member_value = value; has_value = true; }
        }
    }

    public class ProtoMemberInt64List : ProtoMemberInt64ReadWrite
    {
        private System.Collections.Generic.List<long> m_member_value;

        public ProtoMemberInt64List(uint member_id, bool is_required) :
            base(member_id, true)
        {
            m_member_value = new System.Collections.Generic.List<long>();
        }

        public System.Collections.Generic.List<long> member_value
        {
            get { return m_member_value; }
        }
    }

    public class ProtoMemberBoolReadWrite : ProtoMemberBase
    {
        public ProtoMemberBoolReadWrite(uint member_id, bool is_required) :
            base(member_id, is_required, false)
        {

        }

        public int Serialize(bool member_value, ref MemoryStream out_stream)
        {
            int count = 0;

            // 是必填字段， 或者选填字段有值
            if (is_required || has_value)
            {
                // 写key：m_member_id << 3 | 0（bool对应类型）;
                uint type = member_id << 3 | 0;
                count += ProtoReadWrite.Variant(type, ref out_stream);

                // 写value
                count += ProtoReadWrite.Variant(member_value, ref out_stream);
            }

            return count;
        }

        public int ParseFrom(ref bool member_value, ref MemoryStream int_stream)
        {
            int count = 0;

            // 读key
            uint type = 0;
            count += ProtoReadWrite.DeVariant(ref type, ref int_stream);

            // key中的字段序号匹配，否则恢复int_stream并跳过
            uint temp_member_id = type >> 3;
            if (temp_member_id == member_id)
            {
                // 读value
                count += ProtoReadWrite.DeVariant(ref member_value, ref int_stream);

                // 标记有值
                has_value = true;
            }
            else
            {
                // 恢复流的当前位置
                long curr_stream_len = int_stream.Position;
                int_stream.Seek(curr_stream_len - count, SeekOrigin.Begin);

                count = 0;
            }

            return count;
        }
    }

    public class ProtoMemberBool : ProtoMemberBoolReadWrite
    {
        private bool m_member_value;

        public ProtoMemberBool(uint member_id, bool is_required) :
            base(member_id, is_required)
        {
            m_member_value = false;
        }

        public bool member_value
        {
            get { return m_member_value; }
            set { m_member_value = value; has_value = true; }
        }
    }

    public class ProtoMemberBoolList : ProtoMemberBoolReadWrite
    {
        private System.Collections.Generic.List<bool> m_member_value;

        public ProtoMemberBoolList(uint member_id, bool is_required) :
            base(member_id, true)
        {
            m_member_value = new System.Collections.Generic.List<bool>();
        }

        public System.Collections.Generic.List<bool> member_value
        {
            get { return m_member_value; }
        }
    }

    public class ProtoMemberFloatReadWrite : ProtoMemberBase
    {
        public ProtoMemberFloatReadWrite(uint member_id, bool is_required) :
            base(member_id, is_required, false)
        {

        }

        public int Serialize(float member_value, ref MemoryStream out_stream)
        {
            int count = 0;

            // 是必填字段， 或者选填字段有值
            if (is_required || has_value)
            {
                // 写key：m_member_id << 3 | 0（bool对应类型）;
                uint type = member_id << 3 | 5;
                count += ProtoReadWrite.Variant(type, ref out_stream);

                // 写value
                count += ProtoReadWrite.Float(member_value, ref out_stream);
            }

            return count;
        }

        public int ParseFrom(ref float member_value, ref MemoryStream int_stream)
        {
            int count = 0;

            // 读key
            uint type = 0;
            count += ProtoReadWrite.DeVariant(ref type, ref int_stream);

            // key中的字段序号匹配，否则恢复int_stream并跳过
            uint temp_member_id = type >> 3;
            if (temp_member_id == member_id)
            {
                // 读value
                count += ProtoReadWrite.DeFloat(ref member_value, ref int_stream);

                // 标记有值
                has_value = true;
            }
            else
            {
                // 恢复流的当前位置
                long curr_stream_len = int_stream.Position;
                int_stream.Seek(curr_stream_len - count, SeekOrigin.Begin);

                count = 0;
            }

            return count;
        }
    }

    public class ProtoMemberFloat : ProtoMemberFloatReadWrite
    {
        private float m_member_value;

        public ProtoMemberFloat(uint member_id, bool is_required) :
            base(member_id, is_required)
        {
            m_member_value = 0.0f;
        }

        public float member_value
        {
            get { return m_member_value; }
            set { m_member_value = value; has_value = true; }
        }
    }

    public class ProtoMemberFloatList : ProtoMemberFloatReadWrite
    {
        private System.Collections.Generic.List<float> m_member_value;

        public ProtoMemberFloatList(uint member_id, bool is_required) :
            base(member_id, true)
        {
            m_member_value = new System.Collections.Generic.List<float>();
        }

        public System.Collections.Generic.List<float> member_value
        {
            get { return m_member_value; }
        }
    }

    public class Serializer
    {
        public static T Deserialize<T>(MemoryStream ms, T new_t) where T : IMessage, new()
        {
            new_t.ParseFrom(ref ms);
            return new_t;
        }
        public static void Serialize<T>(MemoryStream ms, T t) where T : IMessage
        {
            t.Serialize(ref ms);
        }
    }
}
