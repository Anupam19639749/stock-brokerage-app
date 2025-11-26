import { Navigate, Outlet } from "react-router-dom";
import { useAppSelector } from "../../hooks/redux-hooks";

const AdminRoute = () => {
  const { isAuthenticated, user } = useAppSelector((state) => state.auth);

  // User must be authenticated AND have the "Admin" role
  const isAdmin = isAuthenticated && user?.role === "Admin";

  return isAdmin ? <Outlet /> : <Navigate to="/" replace />;
};

export default AdminRoute;