#define GRAVITATION  0.0002959122083
#define ESP 0.01 

inline double4 Interaction(double4 PosA, double4 PosB,  double4 AccA)
{             double4 d = PosB - PosA;
			 d.w = 0;
             double invr = rsqrt(d.x*d.x + d.y*d.y + d.z*d.z + ESP);
             double f = PosB.w*invr*invr*invr*GRAVITATION;
             AccA += f*d; // Accumulate acceleration 
          
       return AccA;
}

inline double4 getAcc(double4 p, __local double4* *buffer,  int BufferSize, double4 a)
{            
          
		  for(int j=0; j<BufferSize; j++) { // For ALL cached particle positions ... 
             double4 p2 = *buffer[j]; // Read a cached particle position a = 
			 a = Interaction(p, p2, a);
          }
       return a;
}

__kernel void calcAcc(__global double4* PosM_old,
__global double4* PosM_new,
__global double4* Vel, 
__global double4* acc,
double dt,
double Distance,
double Mass,
__local double4* buffer)

{
	int gti = get_global_id(0);
    int ti = get_local_id(0);

	int wid = get_group_id (0);

    int n = get_global_size(0);
    int nt = get_local_size(0);
    int nb = n/nt;
	
	double4 p = PosM_old[gti];
	double4 v = Vel[gti];

	double g = p.y + Distance;
	g = Mass /(g * g);

	double4 a = (double4)(0.0,0.0,0.0,0.0);

	for(int jb=0; jb < nb; jb++) {
         buffer[ti] = PosM_old[jb*nt+ti]; 
		 barrier(CLK_LOCAL_MEM_FENCE); // Wait for others in the work-group 
         //a = getAcc(p, &buffer, nt, a);
		 for(int j=0; j<nt; j++) { // For ALL cached particle positions ... 
             double4 p2 = buffer[j]; // Read a cached particle position a = 
			 a = Interaction(p, p2, a);
          }
		 barrier(CLK_LOCAL_MEM_FENCE); // Wait for others in work-group
       }

	acc[gti] = a;
	a.y += g;

	p += dt*v + 0.5*dt*dt*a;
	v += dt*a;
	
    PosM_new[gti] = p ;
	PosM_new[gti].w = PosM_old[gti].w;
    Vel[gti] = v;
	
}
