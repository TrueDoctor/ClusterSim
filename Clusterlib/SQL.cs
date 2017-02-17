using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using System.Windows.Forms;

using System.Data.SqlClient;
//using ClusterLib.ClustersimDataSetTableAdapters;
using System.Data.Sql;

namespace ClusterLib
{
    public class SQL
    {
        
        const string conString = @"Data Source=tcp:TARDIS\CLUSTERSIM,49172;Initial Catalog=Clustersim;User Id=Engine;Password=mynona; MultipleActiveResultSets=true;";//initialize connection
        


        public static List<Star> readStars(string table, int step)
        {
            SqlConnection con = new SqlConnection(conString);//connect
            List<Star> Stars = new List<Star>();
            using (SqlCommand cmd = new SqlCommand("SELECT id,[pos x] AS posx,[pos y] AS posy,[pos z] AS posz,[vel x] AS velx,[vel y] AS vely,[vel z] AS velz, mass FROM [" + table + "] WHERE step = " + step+"", con))//retrive all data
            {

                try
                {
                    con.Close();
                    con.Open();//open connection
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        // Check is the reader has any rows at all before starting to read.
                        if (reader.HasRows)
                        {

                            while (reader.Read())
                            {
                                Vector pos = new Vector(new decimal[] { Convert.ToDecimal(reader.GetString(reader.GetOrdinal("posx")).Replace(".", ",")), Convert.ToDecimal(reader.GetString(reader.GetOrdinal("posy")).Replace(".", ",")), Convert.ToDecimal(reader.GetString(reader.GetOrdinal("posz")).Replace(".", ",")) });
                                Vector vel = new Vector(new decimal[] { Convert.ToDecimal(reader.GetString(reader.GetOrdinal("velx")).Replace(".", ",")), Convert.ToDecimal(reader.GetString(reader.GetOrdinal("vely")).Replace(".", ",")), Convert.ToDecimal(reader.GetString(reader.GetOrdinal("velz")).Replace(".", ",")) });

                                Stars.Add(new Star(pos.vec, vel.vec, Convert.ToDecimal(reader.GetString(reader.GetOrdinal("mass")).Replace(".", ",")), reader.GetInt32(reader.GetOrdinal("id"))));
                            }
                            con.Close();
                            return Stars;
                        }
                    }
                    
                }
                catch (Exception e)
                {
                    Console.WriteLine("Abrufen von Daten fehlgeschlagen, vlt. Keine Verbindung zum Server möglich \n" + e.Message + "\n");
                    con.Close();
                }
            }
            
            return null;
        }

        public static bool addRow(Star s, int step, string table)
        {

            SqlConnection con = new SqlConnection(conString);//connect
            //string values = "'"+step+"','"+s.id + "','" + s.pos.vec[0] + "','" + s.pos.vec[1] + "','" + s.pos.vec[2] + "','" + s.vel.vec[0] + "','" + s.vel.vec[1] + "','" + s.vel.vec[2] + "','" + s.getMass()+"'";
            string values = "@step,@id,@posx,@posy,@posz,@velx,@vely,@velz,@mass";
            //Console.WriteLine(values);
            using (SqlCommand cmd = new SqlCommand("INSERT INTO[dbo].[" + table + "] ([step] ,[id], [pos x], [pos y], [pos z], [vel x], [vel y], [vel z], [mass]) VALUES(" + values + ")", con))
            {
                cmd.Parameters.Add(new SqlParameter("@step", step));
                cmd.Parameters.Add(new SqlParameter("@id", s.id));
                cmd.Parameters.Add(new SqlParameter("@posx", s.pos.vec[0]));
                cmd.Parameters.Add(new SqlParameter("@posy", s.pos.vec[1]));
                cmd.Parameters.Add(new SqlParameter("@posz", s.pos.vec[2]));
                cmd.Parameters.Add(new SqlParameter("@velx", s.vel.vec[0]));
                cmd.Parameters.Add(new SqlParameter("@vely", s.vel.vec[1]));
                cmd.Parameters.Add(new SqlParameter("@velz", s.vel.vec[2]));
                cmd.Parameters.Add(new SqlParameter("@mass", s.getMass()));


                try
                {
                    con.Close();
                    con.Open();//open connection
                    cmd.ExecuteNonQuery();
                    con.Close();
                }

                catch (Exception e)
                {
                    Console.WriteLine("Hinzufügen von Spalte zu" + table + "fehlgeschlagen \n" + e.Message + "\n");
                    con.Close();
                    return false;
                }

            }

            con.Close();
            return true;
        }

        public static void deleteStep(string table,int step)
        {
            SqlConnection con = new SqlConnection(conString);//connect
            using (SqlCommand cmd = new SqlCommand("Delete FROM [dbo].[" + table + "] WHERE step = "+step, con))
            {
                try
                {
                    con.Close();
                    con.Open();//open connection
                    cmd.ExecuteNonQuery();
                    con.Close();
                }

                catch (Exception e)
                {
                    Console.WriteLine("Löschen von " + table + " fehlgeschlagen  \n" + e.Message + "\n");
                    con.Close();
                }

            }
        }

        public static void addTable(string name)
        {
            SqlConnection con = new SqlConnection(conString);//connect
            String[] ban = new string[] { "[", "]", "'", ".", "+", "*", "-", "/", "°", "!", "\0", "\b", "\'", "\"", "\n", "\r", "\t", @"\", "%" ,"DROP","Drop","drop","All","DELETE","Delete","delete",";",","};
            foreach(string s in ban)
                name = name.Replace(s,"");
            using (SqlCommand cmd = new SqlCommand("CREATE" +
                " TABLE[dbo]. ["+name+"] ([step][int] NULL,[id][int] NOT NULL,[pos x][varchar](32) NOT NULL," +
                "[pos y][varchar](32) NOT NULL,[pos z][varchar](32) NOT NULL,[vel x][varchar](32) NOT NULL," +
                "[vel y][varchar](32) NOT NULL,[vel z][varchar](32) NOT NULL,[mass][varchar](32) NOT NULL) " +
                "ON [PRIMARY]", con))             
            {
                /*SqlParameter param = new SqlParameter();
                param.ParameterName = "@name";
                param.Value = name;
                cmd.Parameters.Add(param);*/

                try
                {
                    con.Close();
                    con.Open();//open connection
                    cmd.ExecuteNonQuery();
                    con.Close();
                }

                catch (Exception e)
                {
                    Console.WriteLine("Hinzufügen von +" + name + " fehlgeschlagen \n" + e.Message + "\n");
                    
                    con.Close();
                }

            }
        }

        public static void dropTable(string table)
        {
            SqlConnection con = new SqlConnection(conString);//connect
            using (SqlCommand cmd = new SqlCommand("DROP TABLE[dbo].[" + table + "]", con))
            {
                try
                {
                    con.Close();
                    con.Open();//open connection
                    cmd.ExecuteNonQuery();
                    con.Close();
                }

                catch (Exception e)
                {
                    Console.WriteLine("Löschen von "+table+" fehlgeschlagen  \n" + e.Message + "\n");
                    con.Close();
                }

            }
        }

        public static List<String> readTables()
        {
            SqlConnection con = new SqlConnection(conString);//connect
            List<String> List = new List<String>();
            using (SqlCommand cmd = new SqlCommand("SELECT * FROM   INFORMATION_SCHEMA.TABLES WHERE  TABLE_TYPE = 'BASE TABLE'", con))//retrive all data
            {
                try
                {
                    con.Close();
                    con.Open();//open connection
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        // Check is the reader has any rows at all before starting to read.
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                List.Add(reader.GetString(reader.GetOrdinal("TABLE_NAME")));
                            }
                            con.Close();
                            return List;
                        }
                    }

                }
                catch (Exception e)
                {
                    Console.WriteLine("Abrufen von Tabellennahmen fehlgeschlagen, vlt. Keine Verbindung zum Server möglich \n" + e.Message + "\n");
                    con.Close();
                }
            }
            return null;
        }
        public static int lastStep(string table)
        {
            SqlConnection con = new SqlConnection(conString);//connect
            int i = -1;
            using (SqlCommand cmd = new SqlCommand("SELECT MAX(step) AS step FROM[dbo].[" + table + "]" , con))
            {
                try
                {
                    con.Close();
                    con.Open();//open connection
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while(reader.Read())
                            i = reader.GetInt32(reader.GetOrdinal("step"));
                    }
                    con.Close();
                }

                catch (Exception e)
                {
                    Console.WriteLine("konnte letzten Schritt von +"+table+" nicht abfragen  \n" + e.Message + "\n");
                    con.Close();
                }

            }
            return i;
        }

        public static int firstStep(string table)
        {
            SqlConnection con = new SqlConnection(conString);//connect
            int i = -1;
            using (SqlCommand cmd = new SqlCommand("SELECT MIN(step) AS step FROM[dbo].[" + table + "]", con))
            {
                try
                {
                    con.Close();
                    con.Open();//open connection
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                            i = reader.GetInt32(reader.GetOrdinal("step"));
                    }
                    con.Close();
                }

                catch (Exception e)
                {
                    Console.WriteLine("konnte letzten Schritt von +" + table + " nicht abfragen  \n" + e.Message + "\n");
                    con.Close();
                }

            }
            return i;
        }

        public static int starsCount(string table, int step = -1)
        {
            SqlConnection con = new SqlConnection(conString);//connect
            int i = -1;
            string com;
            if (step == -1)
                com = "SELECT MAX(id) AS id FROM[dbo].[" + table + "]";
            else
                com = "SELECT MAX(id) AS id FROM[dbo].[" + table + "] WHERE step = " + step;
            using (SqlCommand cmd = new SqlCommand(com, con))
            {
                try
                {
                    con.Close();
                    con.Open();//open connection
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                            i = reader.GetInt32(reader.GetOrdinal("id"));
                    }
                    con.Close();
                }

                catch (Exception e)
                {
                    Console.WriteLine("Konnte anzahl der Sterne von +" + table + " nicht abfragen, ist diese leer?  \n" + e.Message + "\n");
                    con.Close();
                }
                

            }
            return i;
        }
    }
}

