#ifndef PCH_H
#define PCH_H

#include "framework.h"
#include "mkl.h"

extern "C" _declspec(dllexport) int Global(const float* arr, const int nx, const int ny, const double step, const float* left, const float* right, const int n_int, float* res);

#endif //PCH_H