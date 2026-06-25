using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

namespace RoslynPad.ViewModels
{
    /// <summary>
    /// 通知对象基类，提供属性变更通知和数据验证功能。
    /// </summary>
    /// <remarks>
    /// <para>
    /// 此抽象类实现 <see cref="INotifyPropertyChanged"/> 和 <see cref="INotifyDataErrorInfo"/> 接口，
    /// 为所有视图模型提供统一的基础设施。
    /// </para>
    /// <para>
    /// 主要功能：
    /// <list type="bullet">
    ///   <item><description>属性变更通知（通过 <see cref="SetProperty{T}"/> 方法）</description></item>
    ///   <item><description>数据验证错误管理</description></item>
    ///   <item><description>线程安全的错误集合管理</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    public abstract class NotificationObject : INotifyPropertyChanged, INotifyDataErrorInfo
    {
        /// <summary>
        /// 当属性值变更时触发。
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// 设置属性值并在值变更时触发通知。
        /// </summary>
        /// <typeparam name="T">属性类型。</typeparam>
        /// <param name="field">属性的后备字段引用。</param>
        /// <param name="value">新值。</param>
        /// <param name="propertyName">属性名称（自动获取）。</param>
        /// <returns>若值发生变更返回 <see langword="true"/>；否则返回 <see langword="false"/>。</returns>
        protected bool SetProperty<T>(
            [NotNullIfNotNull(nameof(value))] ref T field,
            T value,
            [CallerMemberName] string? propertyName = null)
        {
            if (!EqualityComparer<T>.Default.Equals(field, value))
            {
                field = value;
                OnPropertyChanged(propertyName);
                return true;
            }

            return false;
        }

        /// <summary>
        /// 触发属性变更事件。
        /// </summary>
        /// <param name="propertyName">变更的属性名称。</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #region 数据验证

        /// <summary>
        /// 属性错误字典。
        /// </summary>
        private ConcurrentDictionary<string, List<ErrorInfo>>? _propertyErrors;

        /// <summary>
        /// 设置指定属性的错误。
        /// </summary>
        /// <param name="propertyName">属性名称。</param>
        /// <param name="id">错误标识（用于更或移除特定错误）。</param>
        /// <param name="message">错误消息。</param>
        protected void SetError(string propertyName, string id, string message)
        {
            if (_propertyErrors == null)
            {
                LazyInitializer.EnsureInitialized(
                    ref _propertyErrors,
                    () => new ConcurrentDictionary<string, List<ErrorInfo>>());
            }

            var errors = _propertyErrors.GetOrAdd(propertyName, _ => []);
            errors.RemoveAll(e => e.Id == id);
            errors.Add(new ErrorInfo(id, message));

            OnErrorsChanged(propertyName);
        }

        /// <summary>
        /// 清除指定属性的特定错误。
        /// </summary>
        /// <param name="propertyName">属性名称。</param>
        /// <param name="id">错误标识。</param>
        protected void ClearError(string propertyName, string id)
        {
            if (_propertyErrors == null)
            {
                return;
            }

            _propertyErrors.TryGetValue(propertyName, out var errors);
            if (errors?.RemoveAll(e => e.Id == id) > 0)
            {
                OnErrorsChanged(propertyName);
            }
        }

        /// <summary>
        /// 清除指定属性的所有错误。
        /// </summary>
        /// <param name="propertyName">属性名称。</param>
        protected void ClearErrors(string propertyName)
        {
            if (_propertyErrors == null)
            {
                return;
            }

            _propertyErrors.TryGetValue(propertyName, out var errors);
            if (errors?.Count > 0)
            {
                errors.Clear();
                OnErrorsChanged(propertyName);
            }
        }

        /// <inheritdoc/>
        public IEnumerable GetErrors(string? propertyName)
        {
            if (propertyName == null)
            {
                return Array.Empty<ErrorInfo>();
            }

            List<ErrorInfo>? errors = null;
            _propertyErrors?.TryGetValue(propertyName, out errors);
            return errors?.AsEnumerable() ?? [];
        }

        /// <inheritdoc/>
        public bool HasErrors => _propertyErrors?.Any(c => c.Value.Count != 0) == true;

        /// <summary>
        /// 当验证错误变更时触发。
        /// </summary>
        public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

        /// <summary>
        /// 触发错误变更事件。
        /// </summary>
        /// <param name="propertyName">属性名称。</param>
        protected virtual void OnErrorsChanged(string propertyName)
        {
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }

        /// <summary>
        /// 表示验证错误信息。
        /// </summary>
        /// <param name="id">错误标识。</param>
        /// <param name="message">错误消息。</param>
        protected class ErrorInfo(string id, string message)
        {
            /// <summary>
            /// 获取错误标识。
            /// </summary>
            public string Id { get; } = id;

            /// <summary>
            /// 获取错误消息。
            /// </summary>
            public string Message { get; } = message;

            /// <summary>
            /// 返回错误消息。
            /// </summary>
            /// <returns>错误消息字符串。</returns>
            public override string ToString() => Message;
        }

        #endregion
    }
}
