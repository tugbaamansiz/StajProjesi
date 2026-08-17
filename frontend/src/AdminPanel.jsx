import { useEffect, useMemo, useState } from "react";
import "./AdminPanel.css";

const API_URL = "http://localhost:5166/api/Admin";

function AdminPanel({ token, onBack }) {
  const [activePage, setActivePage] = useState("users");

  const [users, setUsers] = useState([]);
  const [roles, setRoles] = useState([]);
  const [permissions, setPermissions] = useState([]);

  const [selectedUser, setSelectedUser] = useState(null);
  const [selectedRole, setSelectedRole] = useState(null);

  const [userPermissions, setUserPermissions] = useState({
    rolePermissions: [],
    directPermissions: []
  });

  const [loading, setLoading] = useState(false);

  // =====================================================
  // AUTH HEADER
  // =====================================================

  const authHeaders = useMemo(
    () => ({
      Authorization: `Bearer ${token}`,
      "Content-Type": "application/json"
    }),
    [token]
  );


  // =====================================================
  // API HELPER
  // =====================================================

  const request = async (url, options = {}) => {
    const response = await fetch(url, {
      ...options,
      headers: {
        ...authHeaders,
        ...(options.headers || {})
      }
    });

    let data = null;

    try {
      data = await response.json();
    } catch {
      data = null;
    }

    if (!response.ok) {
      throw new Error(
        data?.message || "İşlem sırasında bir hata oluştu."
      );
    }

    return data;
  };


  // =====================================================
  // LOAD USERS
  // =====================================================

  const loadUsers = async () => {
    try {
      const data = await request(`${API_URL}/users`);
      setUsers(data || []);
    } catch (error) {
      console.error("Kullanıcılar alınamadı:", error);
      alert(error.message);
    }
  };


  // =====================================================
  // LOAD ROLES
  // =====================================================

  const loadRoles = async () => {
    try {
      const data = await request(`${API_URL}/roles`);
      setRoles(data || []);
    } catch (error) {
      console.error("Roller alınamadı:", error);
      alert(error.message);
    }
  };


  // =====================================================
  // LOAD PERMISSIONS
  // =====================================================

  const loadPermissions = async () => {
    try {
      const data = await request(`${API_URL}/permissions`);
      setPermissions(data || []);
    } catch (error) {
      console.error("Yetkiler alınamadı:", error);
      alert(error.message);
    }
  };


  // =====================================================
  // LOAD USER PERMISSIONS
  // =====================================================

  const loadUserPermissions = async (userId) => {
    try {
      const data = await request(
        `${API_URL}/users/${userId}/permissions`
      );

      setUserPermissions({
        rolePermissions: data?.rolePermissions || [],
        directPermissions: data?.directPermissions || []
      });
    } catch (error) {
      console.error(
        "Kullanıcı yetkileri alınamadı:",
        error
      );

      alert(error.message);
    }
  };


  // =====================================================
  // INITIAL LOAD
  // =====================================================

  useEffect(() => {
    if (!token) {
      return;
    }

    const loadAll = async () => {
      setLoading(true);

      await Promise.all([
        loadUsers(),
        loadRoles(),
        loadPermissions()
      ]);

      setLoading(false);
    };

    loadAll();
  }, [token]);


  // =====================================================
  // SELECT USER
  // =====================================================

  const handleSelectUser = async (user) => {
    setSelectedUser(user);

    await loadUserPermissions(user.id);
  };


  // =====================================================
  // SELECT ROLE
  // =====================================================

  const handleSelectRole = (role) => {
    setSelectedRole(role);
  };


  // =====================================================
  // CREATE USER
  // =====================================================

  const createUser = async () => {
    const username = window.prompt(
      "Yeni kullanıcının kullanıcı adını girin:"
    );

    if (username === null || username.trim() === "") {
      return;
    }

    const password = window.prompt(
      "Yeni kullanıcının şifresini girin:"
    );

    if (password === null || password.trim() === "") {
      return;
    }

    try {
      await request(`${API_URL}/users`, {
        method: "POST",
        body: JSON.stringify({
          username: username.trim(),
          password: password
        })
      });

      alert("Kullanıcı başarıyla oluşturuldu.");

      await loadUsers();
    } catch (error) {
      alert(error.message);
    }
  };


  // =====================================================
  // UPDATE USER
  // =====================================================

  const updateUser = async (user) => {
    const username = window.prompt(
      "Kullanıcı adını güncelle:",
      user.username
    );

    if (username === null || username.trim() === "") {
      return;
    }

    const changePassword = window.confirm(
      "Şifreyi de değiştirmek ister misiniz?"
    );

    let password = null;

    if (changePassword) {
      password = window.prompt(
        "Yeni şifreyi girin:"
      );

      if (password === null || password.trim() === "") {
        return;
      }
    }

    try {
      await request(`${API_URL}/users/${user.id}`, {
        method: "PUT",
        body: JSON.stringify({
          username: username.trim(),
          password,
          isActive: user.isActive
        })
      });

      alert("Kullanıcı güncellendi.");

      await loadUsers();
    } catch (error) {
      alert(error.message);
    }
  };


  // =====================================================
  // DELETE USER
  // =====================================================

  const deleteUser = async (user) => {
    const confirmed = window.confirm(
      `"${user.username}" kullanıcısını sistemden çıkarmak istediğinize emin misiniz?`
    );

    if (!confirmed) {
      return;
    }

    try {
      await request(`${API_URL}/users/${user.id}`, {
        method: "DELETE"
      });

      if (selectedUser?.id === user.id) {
        setSelectedUser(null);
        setUserPermissions({
          rolePermissions: [],
          directPermissions: []
        });
      }

      alert("Kullanıcı sistemden çıkarıldı.");

      await loadUsers();
    } catch (error) {
      alert(error.message);
    }
  };


  // =====================================================
  // CREATE ROLE
  // =====================================================

  const createRole = async () => {
    const name = window.prompt(
      "Yeni rolün adını girin:"
    );

    if (name === null || name.trim() === "") {
      return;
    }

    const description = window.prompt(
      "Rol açıklamasını girin:"
    );

    try {
      await request(`${API_URL}/roles`, {
        method: "POST",
        body: JSON.stringify({
          name: name.trim(),
          description:
            description?.trim() || ""
        })
      });

      alert("Rol başarıyla oluşturuldu.");

      await loadRoles();
    } catch (error) {
      alert(error.message);
    }
  };


  // =====================================================
  // DELETE ROLE
  // =====================================================

  const deleteRole = async (role) => {
    if (role.name === "Admin") {
      alert(
        "Admin rolü silinemez."
      );
      return;
    }

    const confirmed = window.confirm(
      `"${role.name}" rolünü silmek istediğinize emin misiniz?`
    );

    if (!confirmed) {
      return;
    }

    try {
      await request(`${API_URL}/roles/${role.id}`, {
        method: "DELETE"
      });

      if (selectedRole?.id === role.id) {
        setSelectedRole(null);
      }

      alert("Rol silindi.");

      await loadRoles();
    } catch (error) {
      alert(error.message);
    }
  };


  // =====================================================
  // ASSIGN ROLE TO USER
  // =====================================================

  const assignRoleToUser = async (roleId) => {
    if (!selectedUser) {
      alert("Önce bir kullanıcı seçin.");
      return;
    }

    try {
      await request(
        `${API_URL}/users/${selectedUser.id}/roles/${roleId}`,
        {
          method: "POST"
        }
      );

      alert("Rol kullanıcıya atandı.");

      await loadUsers();
      await loadUserPermissions(selectedUser.id);
    } catch (error) {
      alert(error.message);
    }
  };


  // =====================================================
  // REMOVE ROLE FROM USER
  // =====================================================

  const removeRoleFromUser = async (roleId) => {
    if (!selectedUser) {
      return;
    }

    try {
      await request(
        `${API_URL}/users/${selectedUser.id}/roles/${roleId}`,
        {
          method: "DELETE"
        }
      );

      alert("Rol kullanıcıdan kaldırıldı.");

      await loadUsers();
      await loadUserPermissions(selectedUser.id);
    } catch (error) {
      alert(error.message);
    }
  };


  // =====================================================
  // ASSIGN PERMISSION TO ROLE
  // =====================================================

  const toggleRolePermission = async (
    permissionId,
    checked
  ) => {
    if (!selectedRole) {
      alert("Önce bir rol seçin.");
      return;
    }

    try {
      if (checked) {
        await request(
          `${API_URL}/roles/${selectedRole.id}/permissions/${permissionId}`,
          {
            method: "POST"
          }
        );
      } else {
        await request(
          `${API_URL}/roles/${selectedRole.id}/permissions/${permissionId}`,
          {
            method: "DELETE"
          }
        );
      }

      await loadRoles();

      const updatedRoles =
        await fetch(`${API_URL}/roles`, {
          headers: authHeaders
        }).then((res) => res.json());

      const updatedRole =
        updatedRoles.find(
          (role) => role.id === selectedRole.id
        );

      if (updatedRole) {
        setSelectedRole(updatedRole);
      }

      if (selectedUser) {
        await loadUserPermissions(
          selectedUser.id
        );
      }
    } catch (error) {
      alert(error.message);
    }
  };


  // =====================================================
  // ASSIGN DIRECT USER PERMISSION
  // =====================================================

  const toggleUserPermission = async (
    permissionId,
    checked
  ) => {
    if (!selectedUser) {
      alert("Önce bir kullanıcı seçin.");
      return;
    }

    try {
      if (checked) {
        await request(
          `${API_URL}/users/${selectedUser.id}/permissions/${permissionId}`,
          {
            method: "POST"
          }
        );
      } else {
        await request(
          `${API_URL}/users/${selectedUser.id}/permissions/${permissionId}`,
          {
            method: "DELETE"
          }
        );
      }

      await loadUserPermissions(
        selectedUser.id
      );

      await loadUsers();
    } catch (error) {
      alert(error.message);
    }
  };


  // =====================================================
  // CHECK PERMISSION SOURCE
  // =====================================================

  const rolePermissionIds =
    new Set(
      userPermissions.rolePermissions.map(
        (permission) =>
          permission.permissionId
      )
    );

  const directPermissionIds =
    new Set(
      userPermissions.directPermissions.map(
        (permission) =>
          permission.permissionId
      )
    );


  // =====================================================
  // RENDER
  // =====================================================

  return (
    <div className="admin-panel">

      {/* =================================================
          SIDEBAR
          ================================================= */}

      <aside className="admin-sidebar">

        <div className="admin-logo">
          <div className="admin-logo-icon">
            🛡️
          </div>

          <div>
            <strong>Admin Panel</strong>
            <span>Yönetim Sistemi</span>
          </div>
        </div>


        <nav className="admin-nav">

          <button
            className={
              activePage === "users"
                ? "admin-nav-item active"
                : "admin-nav-item"
            }
            onClick={() =>
              setActivePage("users")
            }
          >
            <i className="pi pi-users"></i>
            <span>Kullanıcı Listesi</span>
          </button>


          <button
            className={
              activePage === "roles"
                ? "admin-nav-item active"
                : "admin-nav-item"
            }
            onClick={() =>
              setActivePage("roles")
            }
          >
            <i className="pi pi-shield"></i>
            <span>Rol Listesi</span>
          </button>


          <button
            className={
              activePage === "permissions"
                ? "admin-nav-item active"
                : "admin-nav-item"
            }
            onClick={() =>
              setActivePage("permissions")
            }
          >
            <i className="pi pi-key"></i>
            <span>Yetki Listesi</span>
          </button>

        </nav>


        <button
          className="admin-back-button"
          onClick={onBack}
        >
          <i className="pi pi-arrow-left"></i>
          Haritaya Dön
        </button>

      </aside>


      {/* =================================================
          MAIN
          ================================================= */}

      <main className="admin-main">

        <header className="admin-header">

          <div>
            <h1>
              {activePage === "users" &&
                "Kullanıcı Yönetimi"}

              {activePage === "roles" &&
                "Rol Yönetimi"}

              {activePage === "permissions" &&
                "Yetki Yönetimi"}
            </h1>

            <p>
              Kullanıcı, rol ve yetki işlemlerini
              buradan yönetebilirsiniz.
            </p>
          </div>


          <div className="admin-header-badge">
            <i className="pi pi-shield"></i>
            Admin
          </div>

        </header>


        {/* =================================================
            USERS PAGE
            ================================================= */}

        {activePage === "users" && (

          <section className="admin-content">

            <div className="admin-card">

              <div className="admin-card-header">

                <div>
                  <h2>Kullanıcı Listesi</h2>
                  <span>
                    Sistemdeki kullanıcılar
                  </span>
                </div>

                <button
                  className="primary-button"
                  onClick={createUser}
                >
                  <i className="pi pi-plus"></i>
                  Kullanıcı Ekle
                </button>

              </div>


              {loading ? (

                <div className="admin-loading">
                  Kullanıcılar yükleniyor...
                </div>

              ) : (

                <div className="admin-table-wrapper">

                  <table className="admin-table">

                    <thead>
                      <tr>
                        <th>ID</th>
                        <th>Kullanıcı</th>
                        <th>Roller</th>
                        <th>Durum</th>
                        <th>İşlemler</th>
                      </tr>
                    </thead>

                    <tbody>

                      {users.map((user) => (

                        <tr
                          key={user.id}
                          className={
                            selectedUser?.id === user.id
                              ? "selected-row"
                              : ""
                          }
                        >

                          <td>
                            #{user.id}
                          </td>

                          <td>
                            <strong>
                              {user.username}
                            </strong>
                          </td>

                          <td>

                            <div className="tag-list">

                              {(user.roles || []).map(
                                (role) => (

                                  <span
                                    key={role.roleId}
                                    className="role-tag"
                                  >
                                    {role.name}
                                  </span>

                                )
                              )}

                              {(!user.roles ||
                                user.roles.length === 0) && (
                                <span className="empty-tag">
                                  Rol yok
                                </span>
                              )}

                            </div>

                          </td>

                          <td>

                            {user.isActive ? (
                              <span className="status active">
                                Aktif
                              </span>
                            ) : (
                              <span className="status passive">
                                Pasif
                              </span>
                            )}

                          </td>

                          <td>

                            <div className="action-buttons">

                              <button
                                className="small-button view"
                                onClick={() =>
                                  handleSelectUser(user)
                                }
                              >
                                <i className="pi pi-eye"></i>
                                Yetkiler
                              </button>

                              <button
                                className="small-button edit"
                                onClick={() =>
                                  updateUser(user)
                                }
                              >
                                <i className="pi pi-pencil"></i>
                                Güncelle
                              </button>

                              <button
                                className="small-button danger"
                                onClick={() =>
                                  deleteUser(user)
                                }
                              >
                                <i className="pi pi-trash"></i>
                                Çıkar
                              </button>

                            </div>

                          </td>

                        </tr>

                      ))}

                    </tbody>

                  </table>

                </div>

              )}

            </div>


            {/* =============================================
                SELECTED USER
                ============================================= */}

            {selectedUser && (

              <div className="admin-card">

                <div className="admin-card-header">

                  <div>
                    <h2>
                      {selectedUser.username}
                    </h2>

                    <span>
                      Kullanıcı Rol ve Yetkileri
                    </span>
                  </div>

                  <button
                    className="close-selection"
                    onClick={() => {
                      setSelectedUser(null);
                      setUserPermissions({
                        rolePermissions: [],
                        directPermissions: []
                      });
                    }}
                  >
                    ×
                  </button>

                </div>


                {/* USER ROLES */}

                <div className="permission-section">

                  <h3>
                    <i className="pi pi-shield"></i>
                    Kullanıcı Rolleri
                  </h3>


                  <div className="assigned-list">

                    {(selectedUser.roles || []).map(
                      (role) => (

                        <div
                          key={role.roleId}
                          className="assigned-item"
                        >

                          <span>
                            {role.name}
                          </span>

                          <button
                            onClick={() =>
                              removeRoleFromUser(
                                role.roleId
                              )
                            }
                          >
                            ×
                          </button>

                        </div>

                      )
                    )}

                    {(!selectedUser.roles ||
                      selectedUser.roles.length === 0) && (
                      <span className="empty-message">
                        Bu kullanıcıya henüz rol atanmadı.
                      </span>
                    )}

                  </div>


                  <div className="assign-row">

                    <select
                      defaultValue=""
                      onChange={(e) => {

                        if (e.target.value) {
                          assignRoleToUser(
                            Number(e.target.value)
                          );

                          e.target.value = "";
                        }

                      }}
                    >

                      <option value="">
                        + Rol Ata
                      </option>

                      {roles
                        .filter(
                          (role) =>
                            !(selectedUser.roles || [])
                              .some(
                                (userRole) =>
                                  userRole.roleId ===
                                  role.id
                              )
                        )
                        .map((role) => (

                          <option
                            key={role.id}
                            value={role.id}
                          >
                            {role.name}
                          </option>

                        ))}

                    </select>

                  </div>

                </div>


                {/* USER PERMISSIONS */}

                <div className="permission-section">

                  <h3>
                    <i className="pi pi-key"></i>
                    Kullanıcı Yetkileri
                  </h3>

                  <p className="permission-info">
                    Rol üzerinden gelen yetkiler
                    tekrar seçilemez ve
                    <strong>
                      {" "}“Rol üzerinden geliyor”
                    </strong>
                    {" "}olarak gösterilir.
                  </p>


                  <div className="permission-grid">

                    {permissions.map(
                      (permission) => {

                        const fromRole =
                          rolePermissionIds.has(
                            permission.id
                          );

                        const directlyAssigned =
                          directPermissionIds.has(
                            permission.id
                          );

                        return (

                          <label
                            key={permission.id}
                            className={
                              fromRole
                                ? "permission-option role-source"
                                : directlyAssigned
                                  ? "permission-option direct-source"
                                  : "permission-option"
                            }
                          >

                            <input
                              type="checkbox"
                              checked={
                                fromRole ||
                                directlyAssigned
                              }
                              disabled={fromRole}
                              onChange={(e) =>
                                toggleUserPermission(
                                  permission.id,
                                  e.target.checked
                                )
                              }
                            />


                            <span className="permission-name">
                              {permission.name}
                            </span>


                            {fromRole && (
                              <span className="source-badge role">
                                Rol üzerinden geliyor
                              </span>
                            )}


                            {!fromRole &&
                              directlyAssigned && (
                                <span className="source-badge direct">
                                  Kullanıcıya özel
                                </span>
                              )}

                          </label>

                        );
                      }
                    )}

                  </div>

                </div>

              </div>

            )}

          </section>

        )}


        {/* =================================================
            ROLES PAGE
            ================================================= */}

        {activePage === "roles" && (

          <section className="admin-content">

            <div className="admin-card">

              <div className="admin-card-header">

                <div>
                  <h2>Rol Listesi</h2>
                  <span>
                    Sistemdeki roller ve yetkileri
                  </span>
                </div>

                <button
                  className="primary-button"
                  onClick={createRole}
                >
                  <i className="pi pi-plus"></i>
                  Rol Ekle
                </button>

              </div>


              <div className="role-grid">

                {roles.map((role) => (

                  <div
                    key={role.id}
                    className={
                      selectedRole?.id === role.id
                        ? "role-card selected"
                        : "role-card"
                    }
                    onClick={() =>
                      handleSelectRole(role)
                    }
                  >

                    <div className="role-card-header">

                      <div className="role-icon">
                        🛡️
                      </div>

                      <div>
                        <h3>
                          {role.name}
                        </h3>

                        <p>
                          {role.description ||
                            "Açıklama bulunmuyor."}
                        </p>
                      </div>

                    </div>


                    <div className="role-permission-count">

                      <i className="pi pi-key"></i>

                      {(role.permissions || []).length}
                      {" "}yetki

                    </div>


                    {role.name !== "Admin" && (

                      <button
                        className="role-delete-button"
                        onClick={(e) => {
                          e.stopPropagation();
                          deleteRole(role);
                        }}
                      >
                        <i className="pi pi-trash"></i>
                        Rolü Sil
                      </button>

                    )}

                  </div>

                ))}

              </div>

            </div>


            {/* =============================================
                ROLE PERMISSIONS
                ============================================= */}

            {selectedRole && (

              <div className="admin-card">

                <div className="admin-card-header">

                  <div>
                    <h2>
                      {selectedRole.name}
                      {" "}Yetkileri
                    </h2>

                    <span>
                      Bu role atanmış yetkileri
                      yönetin.
                    </span>
                  </div>

                  <button
                    className="close-selection"
                    onClick={() =>
                      setSelectedRole(null)
                    }
                  >
                    ×
                  </button>

                </div>


                <div className="permission-grid">

                  {permissions.map(
                    (permission) => {

                      const assigned =
                        (selectedRole.permissions || [])
                          .some(
                            (rolePermission) =>
                              rolePermission.permissionId ===
                              permission.id
                          );

                      return (

                        <label
                          key={permission.id}
                          className={
                            assigned
                              ? "permission-option direct-source"
                              : "permission-option"
                          }
                        >

                          <input
                            type="checkbox"
                            checked={assigned}
                            onChange={(e) =>
                              toggleRolePermission(
                                permission.id,
                                e.target.checked
                              )
                            }
                          />

                          <span className="permission-name">
                            {permission.name}
                          </span>

                          {assigned && (
                            <span className="source-badge direct">
                              Role atanmış
                            </span>
                          )}

                        </label>

                      );

                    }
                  )}

                </div>

              </div>

            )}

          </section>

        )}


        {/* =================================================
            PERMISSIONS PAGE
            ================================================= */}

        {activePage === "permissions" && (

          <section className="admin-content">

            <div className="admin-card">

              <div className="admin-card-header">

                <div>
                  <h2>Yetki Listesi</h2>
                  <span>
                    Sistemde tanımlı tüm yetkiler
                  </span>
                </div>

                <button
                  className="primary-button"
                  onClick={async () => {

                    const name =
                      window.prompt(
                        "Yetki adını girin:"
                      );

                    if (
                      name === null ||
                      name.trim() === ""
                    ) {
                      return;
                    }

                    const description =
                      window.prompt(
                        "Yetki açıklamasını girin:"
                      );

                    try {

                      await request(
                        `${API_URL}/permissions`,
                        {
                          method: "POST",
                          body: JSON.stringify({
                            name: name.trim(),
                            description:
                              description?.trim() ||
                              ""
                          })
                        }
                      );

                      alert(
                        "Yetki başarıyla oluşturuldu."
                      );

                      await loadPermissions();

                    } catch (error) {

                      alert(error.message);

                    }

                  }}
                >
                  <i className="pi pi-plus"></i>
                  Yetki Ekle
                </button>

              </div>


              <div className="permission-list">

                {permissions.map(
                  (permission) => (

                    <div
                      key={permission.id}
                      className="permission-list-item"
                    >

                      <div className="permission-list-icon">
                        🔑
                      </div>

                      <div className="permission-list-info">

                        <strong>
                          {permission.name}
                        </strong>

                        <span>
                          {permission.description ||
                            "Açıklama bulunmuyor."}
                        </span>

                      </div>

                      <span className="permission-id">
                        #{permission.id}
                      </span>

                    </div>

                  )
                )}

              </div>

            </div>

          </section>

        )}

      </main>

    </div>
  );
}

export default AdminPanel;