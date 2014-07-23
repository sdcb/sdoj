using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;

namespace SdojWeb.Models.Identity
{
    public class RoleStore : IQueryableRoleStore<Role, int>
    {
        private readonly ApplicationDbContext db;


        public RoleStore(ApplicationDbContext db)
        {
            this.db = db;
        }


        //// IQueryableRoleStore<UserRole, TKey>


        public IQueryable<Role> Roles
        {
            get { return db.Roles; }
        }


        //// IRoleStore<UserRole, TKey>


        public virtual Task CreateAsync(Role role)
        {
            if (role == null)
            {
                throw new ArgumentNullException("role");
            }


            db.Roles.Add(role);
            return db.SaveChangesAsync();
        }


        public Task DeleteAsync(Role role)
        {
            if (role == null)
            {
                throw new ArgumentNullException("role");
            }


            db.Roles.Remove(role);
            return db.SaveChangesAsync();
        }


        public Task<Role> FindByIdAsync(int roleId)
        {
            return db.Roles.FindAsync(roleId);
        }


        public Task<Role> FindByNameAsync(string roleName)
        {
            return db.Roles.FirstOrDefaultAsync(r => r.Name == roleName);
        }


        public Task UpdateAsync(Role role)
        {
            if (role == null)
            {
                throw new ArgumentNullException("role");
            }


            db.Entry(role).State = EntityState.Modified;
            return db.SaveChangesAsync();
        }


        //// IDisposable
        

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }


        protected virtual void Dispose(bool disposing)
        {
            if (disposing && db != null)
            {
                db.Dispose();
            }
        }
    }
}