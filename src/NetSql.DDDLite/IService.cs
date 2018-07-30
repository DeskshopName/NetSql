﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using NetSql.Entities;
using NetSql.Pagination;

namespace NetSql.DDDLite
{
    public interface IService<TEntity> where TEntity : Entity, new()
    {
        /// <summary>
        /// 新增
        /// </summary>
        /// <param name="entity">实体</param>
        /// <returns></returns>
        Task<bool> AddAsync(TEntity entity);

        /// <summary>
        /// 批量添加
        /// </summary>
        /// <param name="entityList">实体列表</param>
        /// <returns></returns>
        Task<bool> BatchAddtAsync(IList<TEntity> entityList);

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="id">主键</param>
        /// <returns></returns>
        Task<int> RemoveAsync(dynamic id);

        /// <summary>
        /// 根据表达式删除
        /// </summary>
        /// <param name="exp">查询条件</param>
        /// <returns></returns>
        Task<int> RemoveAsync(Expression<Func<TEntity, bool>> exp);

        /// <summary>
        /// 批量删除
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="idList">主键列表</param>
        /// <returns></returns>
        Task<bool> BatchRemoveAsync<T>(IList<T> idList);

        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="entity">实体</param>
        /// <returns></returns>
        Task<int> UpdateAsync(TEntity entity);

        /// <summary>
        /// 根据表达式更新实体
        /// </summary>
        /// <param name="whereExp"></param>
        /// <param name="updateEntity"></param>
        /// <returns></returns>
        Task<int> UpdateAsync(Expression<Func<TEntity, bool>> whereExp, Expression<Func<TEntity, TEntity>> updateEntity);

        /// <summary>
        /// 批量更新
        /// </summary>
        /// <param name="entityList"></param>
        /// <returns></returns>
        Task<bool> BatchUpdateAsync(IList<TEntity> entityList);

        /// <summary>
        /// 查询单个实体
        /// </summary>
        /// <param name="id">主键</param>
        /// <returns></returns>
        Task<TEntity> GetAsync(dynamic id);

        /// <summary>
        /// 根据Lambda表达式查询单挑数据
        /// <para>Note：有多条时返回第一条</para>
        /// </summary>
        /// <param name="where"></param>
        /// <param name="sort">排序</param>
        /// <returns></returns>
        Task<TEntity> GetAsync(Expression<Func<TEntity, bool>> where, ISort sort = null);

        /// <summary>
        /// 查询列表
        /// </summary>
        /// <param name="whereExp">查询条件</param>
        /// <param name="paging">分页</param>
        /// <param name="sort">排序</param>
        /// <returns></returns>
        Task<IEnumerable<TEntity>> Query(Expression<Func<TEntity, bool>> whereExp, Paging paging, ISort sort = null);

        /// <summary>
        /// 查询列表，返回指定列
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="whereExp"></param>
        /// <param name="selectExp"></param>
        /// <param name="paging"></param>
        /// <param name="sort"></param>
        /// <returns></returns>
        Task<IEnumerable<TEntity>> Query<TResult>(Expression<Func<TEntity, bool>> whereExp, Expression<Func<TEntity, TResult>> selectExp, Paging paging, ISort sort = null);
    }
}
