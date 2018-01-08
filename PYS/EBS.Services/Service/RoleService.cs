using EBS.Data;
using EBS.Data.Result;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EBS.Services.Service
{
    public class RoleService
    {
        public Result<List<Role>> GetAll()
        {
            Result<List<Role>> result;
            try
            {
                using (ModelContext db = new ModelContext())
                {
                    var roleList = db.Role.ToList();
                    result = new Result<List<Role>>(roleList);
                }
            }
            catch (Exception ex)
            {
                result = new Result<List<Role>>(false, ex.ToString());
            }
            return result;
        }

        public Result<Role> GetByID(int id)
        {
            Result<Role> result;
            try
            {
                using (ModelContext db = new ModelContext())
                {
                    var role = db.Role.Include("Auth").Include("User").FirstOrDefault(i=>i.ID == id);
                    if (role != null)
                    {
                        result = new Result<Role>(role);
                    }
                    else
                    {
                        result = new Result<Role>(false, "Bulunamadı!");
                    }
                }
            }
            catch (Exception ex)
            {
                result = new Result<Role>(false, ex.ToString());
            }
            return result;
        }

        public Result<string> Create(Role model)
        {
            Result<string> result;
            try
            {
                using (ModelContext db = new ModelContext())
                {
                    db.Role.Add(model);
                    db.SaveChanges();
                }
                result = new Result<string>("Başarılı");
            }
            catch (Exception ex)
            {
                result = new Result<string>(false,ex.ToString());
            }
            return result;
        }

        public Result<string> Edit(Role model)
        {
            Result<string> result;
            try
            {
                using (ModelContext db = new ModelContext())
                {
                    Role role = db.Role.FirstOrDefault(i=>i.ID == model.ID);
                    role.Name = model.Name;
                    role.Auth = model.Auth;
                    db.SaveChanges();
                }
                result = new Result<string>("Başarılı");
            }
            catch (Exception ex)
            {
                result = new Result<string>(false, ex.ToString());
            }
            return result;
        }

        public Result<string> Delete(int id)
        {
            Result<string> result;
            try
            {
                using (ModelContext db = new ModelContext())
                {
                    Role role = db.Role.FirstOrDefault(i => i.ID == id);
                    foreach (var i in role.Auth)
                    {
                        role.Auth.Remove(i);
                    }
                    var defaultRol = db.Role.Where(i => i.Name == "Default").SingleOrDefault();
                    foreach (var i in role.User)
                    {
                        i.Role = defaultRol;
                    }
                    db.Role.Remove(role);
                    db.SaveChanges();
                }
                result = new Result<string>("Başarılı");
            }
            catch (Exception ex)
            {
                result = new Result<string>(false, ex.ToString());
            }
            return result;
        }
    }
}
