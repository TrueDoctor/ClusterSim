
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
__local double4* buffer)

{
	int gti = get_global_id(0);
    int ti = get_local_id(0);

	int wid = get_group_id (0);

    int n = get_global_size(0);
    int nt = get_local_size(0);
    int nb = n/nt;
	
	int test = 0;
	double4 p = PosM_old[gti];
	double4 v = Vel[gti];

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

	double4 kap = dt*v;
    double4 kav = dt*a;
	
	acc[gti] = a;
	barrier(CLK_LOCAL_MEM_FENCE);
	////////////////////////////////////////////
	a = (double4)(0.0,0.0,0.0,0.0);

	for(int jb=0; jb < nb; jb++) {
         buffer[ti] = PosM_old[jb*nt+ti]; 
		 barrier(CLK_LOCAL_MEM_FENCE); // Wait for others in the work-group 
         //a = getAcc(p, &buffer, nt, a);
		 for(int j=0; j<nt; j++) { // For ALL cached particle positions ... 
             double4 p2 = buffer[j]; // Read a cached particle position a = 
			 a = Interaction(kap, p2, a);
          }
		 barrier(CLK_LOCAL_MEM_FENCE); // Wait for others in work-group
       }

	double4 ffp = dt*kav;
    double4 ffv = dt*a;
	barrier(CLK_LOCAL_MEM_FENCE);
	////////////////////////////////////////
	a = (double4)(0.0,0.0,0.0,0.0);

	double4 tempP = p + 1.0/3.0 * kap + 1.0 / 18.0 * ffp;
	double4 tempV = v + 1.0/3.0 * kav + 1.0 / 18.0 * ffv;

	for(int jb=0; jb < nb; jb++) {
         buffer[ti] = PosM_old[jb*nt+ti]; 
		 barrier(CLK_LOCAL_MEM_FENCE); 
		 for(int j=0; j<nt; j++) { 
             double4 p2 = buffer[j]; 
			 a = Interaction(tempP, p2, a);
          }
		 barrier(CLK_LOCAL_MEM_FENCE); 
       }

	double4 kbp = dt * tempV;
    double4 kbv = dt * a;
	barrier(CLK_LOCAL_MEM_FENCE);
	////////////////////////////////////
	a = (double4)(0.0,0.0,0.0,0.0);

	tempP = p - 1.216 * kap + 252.0 / 125.0 * kbp - 44.0 / 125.0 * ffp;
	tempV = v - 1.216 * kav + 252.0 / 125.0 * kbv - 44.0 / 125.0 * ffv;

	for(int jb=0; jb < nb; jb++) {
         buffer[ti] = PosM_old[jb*nt+ti]; 
		 barrier(CLK_LOCAL_MEM_FENCE); 
		 for(int j=0; j<nt; j++) { 
             double4 p2 = buffer[j]; 
			 a = Interaction(tempP, p2, a);
          }
		 barrier(CLK_LOCAL_MEM_FENCE); 
       }

	double4 kcp = dt * tempV;
    double4 kcv = dt * a;
	barrier(CLK_LOCAL_MEM_FENCE);
	////////////////////////////////////
	a = (double4)(0.0,0.0,0.0,0.0);

	tempP = p + 9.5 * kap - 72.0 / 7.0 * kbp + 25.0 / 14.0 * kcp + 44.0 / 125.0 * ffp;
	tempV = v + 9.5 * kav - 72.0 / 7.0 * kbv + 25.0 / 14.0 * kcv + 44.0 / 125.0 * ffv;

	for(int jb=0; jb < nb; jb++) {
         buffer[ti] = PosM_old[jb*nt+ti]; 
		 barrier(CLK_LOCAL_MEM_FENCE); 
		 for(int j=0; j<nt; j++) { 
             double4 p2 = buffer[j]; 
			 a = Interaction(tempP, p2, a);
          }
		 barrier(CLK_LOCAL_MEM_FENCE); 
       }

	double4 kdp = dt * tempV; 
    double4 kdv = dt*a;

	double4 fp = 5.0 / 48.0 * kap + 27.0 / 56.0 * kbp + 125.0 / 336.0 * kcp + 1.0 / 24.0 * kdp;
	double4 fv = 5.0 / 48.0 * kav + 27.0 / 56.0 * kbv + 125.0 / 336.0 * kcv + 1.0 / 24.0 * kdv;

    PosM_new[gti] = p + fp;
	PosM_new[gti].w = PosM_old[gti].w;
    Vel[gti] = fv + v;
	
}
