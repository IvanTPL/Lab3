#include "pch.h"

extern "C" _declspec(dllexport) int Global(const float* arr, const int nx, const int ny, const double step, const float* left, const float* right, const int n_int, float* res)
{
	DFTaskPtr task;
	float bounds[2] = { 0, (nx - 1) * step };
	int status = dfsNewTask1D(&task, nx, bounds, DF_UNIFORM_PARTITION, ny * 2, arr, DF_NO_HINT);
	if (status != DF_STATUS_OK)
		return status;
	float* scoeff = new float[ny * 2 * DF_PP_CUBIC * (nx - 1)];
	status = dfsEditPPSpline1D(task, DF_PP_CUBIC, DF_PP_NATURAL, DF_BC_FREE_END, NULL, DF_NO_IC, NULL, scoeff, DF_NO_HINT);
	if (status != DF_STATUS_OK)
		return status;
	status = dfsConstruct1D(task, DF_PP_SPLINE, DF_METHOD_STD);
	if (status != DF_STATUS_OK)
		return status;
	status = dfsIntegrate1D(task, DF_METHOD_PP, n_int, left, DF_NO_HINT, right, DF_NO_HINT, NULL, NULL, res, DF_NO_HINT);
	if (status != DF_STATUS_OK)
		return status;
	status = dfDeleteTask(&task);
	return status;
}

