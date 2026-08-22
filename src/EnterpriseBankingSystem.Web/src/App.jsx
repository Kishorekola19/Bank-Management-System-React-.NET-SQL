import React, { useState } from 'react';
import { AuthProvider, useAuth } from './context/AuthContext';
import { Navbar } from './components/Navbar';
import { AuthPage } from './pages/AuthPage';
import { DashboardPage } from './pages/DashboardPage';
import { AccountsPage } from './pages/AccountsPage';
import { CardsPage } from './pages/CardsPage';
import { TransferPage } from './pages/TransferPage';
import { LoansPage } from './pages/LoansPage';
import { CustomersPage } from './pages/CustomersPage';
import { AuditPage } from './pages/AuditPage';
import { PendingApprovalsPage } from './pages/PendingApprovalsPage';

const MainContent = () => {
  const { user, loading } = useAuth();
  const [activeTab, setActiveTab] = useState('dashboard');

  if (loading) {
    return (
      <div className="min-h-screen bg-slate-900 flex items-center justify-center text-white">
        <div className="flex items-center gap-3">
          <div className="w-6 h-6 border-2 border-blue-500 border-t-transparent rounded-full animate-spin"></div>
          <span className="text-sm font-semibold">Initializing Security Portal...</span>
        </div>
      </div>
    );
  }

  if (!user) {
    return <AuthPage />;
  }

  return (
    <div className="min-h-screen bg-slate-100 text-slate-900 font-sans">
      <Navbar activeTab={activeTab} setActiveTab={setActiveTab} />
      <main className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        {activeTab === 'dashboard' && <DashboardPage setActiveTab={setActiveTab} />}
        {activeTab === 'approvals' && user.role === 'Admin' && <PendingApprovalsPage />}
        {activeTab === 'accounts' && <AccountsPage />}
        {activeTab === 'cards' && <CardsPage />}
        {activeTab === 'transfer' && <TransferPage />}
        {activeTab === 'loans' && <LoansPage />}
        {activeTab === 'customers' && user.role === 'Admin' && <CustomersPage />}
        {activeTab === 'audit' && user.role === 'Admin' && <AuditPage />}
      </main>
      <footer className="border-t border-slate-200 py-6 text-center text-xs text-slate-500 font-medium">
        Apex Enterprise Banking System • Secure Digital Financial Services
      </footer>
    </div>
  );
};

export default function App() {
  return (
    <AuthProvider>
      <MainContent />
    </AuthProvider>
  );
}
