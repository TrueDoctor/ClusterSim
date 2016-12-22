using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using ClusterSim.ClustersimDataSetTableAdapters;

namespace ClusterSim
{
    class SQL
    {
        

        public SQL() //constructor
        {

        }

        public List<Star> readStars(string table, string Database = "ClusterSim")
        {
            List<Star> Stars = new List<Star>();
            String conString = @"Data Source=tcp:TARDIS\CLUSTERSIM,49172;Initial Catalog="+Database+";User Id=Engine;Password=mynona; MultipleActiveResultSets=true;";//initialize connection
            SqlConnection con = new SqlConnection(conString);//connect
            using (SqlCommand cmd = new SqlCommand("SELECT id,[pos x] AS posx,[pos y] AS posy,[pos z] AS posz,[vel x] AS velx,[vel y] AS vely,[vel z] AS velz,[acc x] AS accx,[acc y] AS accy,[acc z] AS accz, mass FROM " + table, con))//retrive all data
                {
                    for (;;)//wait for connection
                        try
                        {
                            con.Open();//open connection
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                // Check is the reader has any rows at all before starting to read.
                                if (reader.HasRows)
                                {
                                    // Read advances to the next row.
                                    while (reader.Read())
                                    {
                                        Vector pos = new Vector(new double[] { reader.GetDouble(reader.GetOrdinal("posx")), reader.GetDouble(reader.GetOrdinal("posy")), reader.GetDouble(reader.GetOrdinal("posz")) });
                                        Vector vel = new Vector(new double[] { reader.GetDouble(reader.GetOrdinal("velx")), reader.GetDouble(reader.GetOrdinal("vely")), reader.GetDouble(reader.GetOrdinal("velz")) });
                                        Vector acc = new Vector(new double[] { reader.GetDouble(reader.GetOrdinal("accx")), reader.GetDouble(reader.GetOrdinal("accy")), reader.GetDouble(reader.GetOrdinal("accz")) });
                                        Stars.Add(new Star(pos.vec, vel.vec, acc.vec, reader.GetDouble(reader.GetOrdinal("mass"))));
                                    }

                                    return Stars;
                                }
                                else return null;
                            }
                        }

                        catch { }

                }
            }

            public void writeStar(int id,Star s, string table, string Database = "ClusterSim")
            {
            InitialAdapter test = new InitialAdapter();
            test.Insert(id, s.getPos().vec[0], s.getPos().vec[1], s.getPos().vec[2], s.getVel().vec[0], s.getVel().vec[1], s.getVel().vec[2], s.getAcc().vec[0], s.getAcc().vec[1], s.getAcc().vec[2], s.getMass());
        }
        }
    }
