import { Navigate, Outlet } from "react-router-dom";
import { useAppSelector } from "../../hooks/redux-hooks";

const ProtectedRoute = () => {
  const { isAuthenticated } = useAppSelector((state) => state.auth);

  // If the user is authenticated, show the child page (e.g., HomePage)
  // Otherwise, redirect them to the /login page
  return isAuthenticated ? <Outlet /> : <Navigate to="/login" replace />;
};

export default ProtectedRoute;