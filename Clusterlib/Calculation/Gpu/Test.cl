
double3 Interaction(double4 bi, double4 bj,  double3 ai)
{
	double3 r;
	// r_ij  [3 FLOPS]
	r.x = bj.x - bi.x;
	r.y = bj.y - bi.y;
	r.z = bj.z - bi.z;
	// distSqr = dot(r_ij, r_ij) + EPS^2  [6 FLOPS]
	double distSqr = r.x * r.x + r.y * r.y + r.z * r.z + 0.01;
	// invDistCube =1/distSqr^(3/2)  [4 FLOPS (2 mul, 1 sqrt, 1 inv)]
	double distSixth = distSqr * distSqr * distSqr;
	double invDistCube = 1.0 / sqrt(distSixth);
	// s = m_j * invDistCube [1 FLOP]
	double s = bj.w * invDistCube;
	// a_i =  a_i + s * r_ij [6 FLOPS]
	ai.x += r.x * s;
	ai.y += r.y * s;
	ai.z += r.z * s;
	printf("%v",ai);
	return ai;
}

__kernel void calcAcc(__global double4* input,
__global double4* objects, 
__global int* instructions, 
__global int* from, 
__global int* workSize, 
__global int* workGroupSize, 
__global double4* acc,
double esp,
__local double4* buffer)

{
	int gti = get_global_id(0);
    int ti = get_local_id(0);

	int wid = get_group_id (0);

    int n = get_global_size(0);
    int nt = get_local_size(0);
    int nb = n/nt;
	
	//nt = workGroupSize[wid];
	//size_t i = get_global_id(0);
	//size_t m = get_global_size(0);
	int test = 0;
	double3 output;
	double4 work;// = objects[j];
	double4 current = input[gti];

	double4 a = (double4)(0.0,0.0,0.0,0.0);

	for(int jb=0; jb < workSize[wid]/nt; jb++) { 

         buffer[ti] = objects[from[wid]+workSize[wid]*jb+ti]; 
          barrier(CLK_LOCAL_MEM_FENCE); // Wait for others in the work-group 

          for(int j=0; j<nt; j++) { // For ALL cached particle positions ... 
             double4 p2 = buffer[j]; // Read a cached particle position 
             double4 d = p2 - current;
             double invr = rsqrt(d.x*d.x + d.y*d.y + d.z*d.z + 0.01);
             double f = p2.w*invr*invr*invr;
             a += f*d; // Accumulate acceleration 
			 test++;
          }

          barrier(CLK_LOCAL_MEM_FENCE); // Wait for others in work-group 
       }

	/*for (j = from[i]; j < from[i] + to[i] ; j = j + 1)
	{
		printf("from: %i", j);
		work = objects[instructions[j]];
		output = Interaction(current, work, output);
	}*/
	a.x = esp;
	//a.y = test;
	//a.z = wid;
	a.w = gti;
	acc[gti] = a;
	
}

;