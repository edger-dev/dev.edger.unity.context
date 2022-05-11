using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using Edger.Unity;
using Edger.Unity.Context;

namespace Edger.Unity.Grid {
    public abstract class Grid2D<T> : Aspect {
        public int Rows { get; private set; }
        public int Cols { get; private set; }

        public Vector2 CellSize { get; private set; }
        public Vector2 Center { get; private set; }

        private T[] _Cache = null;
        public T[] Cache {
            get { return _Cache; }
        }

        private bool _RowsIsEven = false;
        private bool _ColsIsEven = false;

        public bool Setup(int rows, int cols, Vector2 cellSize) {
            if (rows <= 0 || cols <= 0 || cellSize.x <= 0f || cellSize.y <= 0f) {
                return false;
            }
            Rows = rows;
            Cols = cols;
            CellSize = cellSize;
            _Cache = null;
            ResetCache();
            return _Cache != null;
        }

        public bool Setup(Vector2 size, Vector2 cellSize) {
            var rows = Mathf.CeilToInt(size.x / cellSize.x);
            var cols = Mathf.CeilToInt(size.y / cellSize.y);
            return Setup(rows, cols, cellSize);
        }

        private void ResetCache() {
            _RowsIsEven = Rows % 2 == 0;
            _ColsIsEven = Cols % 2 == 0;

            Center = new Vector2(
                        Cols * 0.5f * CellSize.x,
                        Rows * 0.5f * CellSize.y);

            _Cache = new T[Rows * Cols];
            for (int i = 0; i < _Cache.Length; i++) {
                _Cache[i] = default(T);
            }
        }

        public void ForEach(Action<int, int, T> callback) {
            if (_Cache == null) return;
            for (int row = 0; row < Rows; row++) {
                for (int col = 0; col < Cols; col++) {
                    callback(row, col, Get(row, col));
                }
            }
        }

        public void ForEachNotNull(Action<int, int, T> callback) {
            if (_Cache == null) return;
            for (int row = 0; row < Rows; row++) {
                for (int col = 0; col < Cols; col++) {
                    T val = Get(row, col);
                    if (val != null) {
                        callback(row, col, val);
                    }
                }
            }
        }

        private int GetIndex(int row, int col) {
            return row * Cols + col;
        }

        public T Get(int row, int col, bool isDebug = false) {
            if (_Cache == null) return default(T);

            T result = default(T);
            int index = GetIndex(row, col);
            if (index >= 0 && index < _Cache.Length) {
                result = _Cache[index];
            } else {
                ErrorOrDebug(isDebug, "Get Failed: [{0}] row = {1}, col = {2}", _Cache.Length, row, col);
            }
            return result;
        }

        public Vector2 GetPos(int row, int col) {
            float x = (col + 0.5f) * CellSize.x;
            float y = (row + 0.5f) * CellSize.y;
            return new Vector2(x, y) - Center;
        }

        public bool GetRowCol(Vector2 pos, out int row, out int col, bool isDebug = false) {
            col = _ColsIsEven
                ? Mathf.FloorToInt((pos.x + Center.x) / CellSize.x)
                : Mathf.RoundToInt((pos.x + Center.x) / CellSize.x);
            row = _RowsIsEven
                ? Mathf.FloorToInt((pos.y + Center.y) / CellSize.y)
                : Mathf.RoundToInt((pos.y + Center.y) / CellSize.y);
            if (row >= 0 && row < Rows && col >= 0 && col < Cols) {
                return true;
            }
            ErrorOrDebug(isDebug, "Out Of Grid: [{0}, {1}] {2} -> pos = {3} -> row = {4}, col = {5}",
                        Rows, Cols, Center, pos, row, col);
            return false;
        }

        public T Get(Vector2 pos, bool isDebug = false) {
            T result = default(T);
            if (_Cache != null) {
                int row, col;
                if (GetRowCol(pos, out row, out col, isDebug)) {
                    result = Get(row, col, isDebug);
                }
            }
            return result;
        }

        public bool Set(int row, int col, T val, out T oldValue) {
            oldValue = default(T);
            if (_Cache == null) return false;

            int index = GetIndex(row, col);
            if (index >= 0 && index < _Cache.Length) {
                oldValue = _Cache[index];
                _Cache[index] = val;
                /*
                if (_OnChanged != null) {
                    TEvt evt = CreateCacheChangedEvt(row, col, val, lastVal);
                    if (evt != null) {
                        _OnChanged.FireEvent(evt);
                    }
                }
                 */
                return true;
            } else {
                Error("Set Failed: [{0}] row = {1}, col = {2}", _Cache.Length, row, col);
            }
            return false;
        }

        public bool Set(int row, int col, T val) {
            T oldValue;
            return Set(row, col, val, out oldValue);
        }

        public bool Set(Vector2 pos, T val) {
            if (_Cache == null) return false;
            int row, col;
            if (GetRowCol(pos, out row, out col)) {
                return Set(row, col, val);
            }
            return false;
        }

        public bool Set(Vector2 pos, T val, out T oldValue) {
            oldValue = default(T);
            if (_Cache == null) return false;
            int row, col;
            if (GetRowCol(pos, out row, out col)) {
                return Set(row, col, val, out oldValue);
            }
            return false;
        }
    }
}